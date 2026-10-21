using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GeoExpande.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeoExpande.Api.Dtos;
using GeoExpande.Api.Models;

namespace GeoExpande.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly GeoExpandeDbContext _context;
    private readonly TokenService _tokenService;
    private readonly IConfiguration _config;

    public AuthController(GeoExpandeDbContext context, TokenService tokenService, IConfiguration config)
    {
        _context = context;
        _tokenService = tokenService;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto dto)
    {
        if (Encoding.UTF8.GetByteCount(dto.Password) > 72)
            return BadRequest("La contraseña no puede superar 72 bytes en UTF-8.");

        var correoNormalizado = dto.Correo.Trim().ToLowerInvariant();
        dto.Nombre = dto.Nombre.Trim();
        dto.Apellido = dto.Apellido?.Trim();
        dto.RazonSocial = dto.RazonSocial?.Trim();
        dto.NombreComercial = dto.NombreComercial.Trim();
        dto.DescripcionNegocio = dto.DescripcionNegocio?.Trim();
        dto.CiudadBase = dto.CiudadBase?.Trim();
        dto.Telefono = dto.Telefono?.Trim();
        dto.SitioWeb = dto.SitioWeb?.Trim();

        var giroValido = await _context.Giros.AnyAsync(giro =>
            giro.IdGiro == dto.IdGiro
            && giro.IdSector == dto.IdSector
            && giro.Activo
            && giro.IdSectorNavigation.Activo);
        if (!giroValido)
            return BadRequest("El giro seleccionado no existe, está inactivo o no pertenece al sector indicado.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var correoExiste = await _context.Usuarios.AnyAsync(u => u.Correo.ToLower() == correoNormalizado);
            if (correoExiste)
                return Conflict("Ya existe un usuario con ese correo.");

            var defaultRoleName = _config["Auth:DefaultRoleName"] ?? "Usuario";
            var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == defaultRoleName);
            if (rol is null)
                return StatusCode(500, $"No existe el rol por defecto '{defaultRoleName}' en la tabla roles.");

            var empresa = new Empresa
            {
                RazonSocial = dto.RazonSocial,
                NombreComercial = dto.NombreComercial,
                IdSector = dto.IdSector,
                IdGiro = dto.IdGiro,
                DescripcionNegocio = dto.DescripcionNegocio,
                CiudadBase = dto.CiudadBase,
                Telefono = dto.Telefono,
                SitioWeb = dto.SitioWeb
            };

            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync();

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Correo = correoNormalizado,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IdRol = rol.IdRol,
                IdEmpresa = empresa.IdEmpresa,
                Activo = true
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var (token, expiresAt) = _tokenService.GenerateToken(usuario, rol.Nombre);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Rol = rol.Nombre
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto dto)
    {
        if (Encoding.UTF8.GetByteCount(dto.Password) > 72)
            return Unauthorized("Correo o contraseña incorrectos.");

        var correoNormalizado = dto.Correo.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.Correo.ToLower() == correoNormalizado);

        if (usuario is null || !usuario.Activo)
            return Unauthorized("Correo o contraseña incorrectos.");

        var passwordValida = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);
        if (!passwordValida)
            return Unauthorized("Correo o contraseña incorrectos.");

        usuario.UltimoAcceso = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        await _context.SaveChangesAsync();

        var (token, expiresAt) = _tokenService.GenerateToken(usuario, usuario.IdRolNavigation.Nombre);

        return Ok(new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            IdUsuario = usuario.IdUsuario,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            Rol = usuario.IdRolNavigation.Nombre
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var authorization = Request.Headers["Authorization"].ToString();
        if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Unauthorized();

        var accessToken = authorization["Bearer ".Length..].Trim();
        var parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        var tokenId = parsedToken.Id;
        var subject = parsedToken.Subject;

        if (string.IsNullOrWhiteSpace(tokenId)
            || !int.TryParse(subject, out var userId)
            || !int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var authenticatedUserId)
            || authenticatedUserId != userId
            || parsedToken.ValidTo <= DateTime.UtcNow)
        {
            return Unauthorized();
        }

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        await _context.TokensRevocados
            .Where(token => token.ExpiraEn <= now)
            .ExecuteDeleteAsync();

        var jtiHash = TokenRevocationService.HashTokenId(tokenId);
        await _context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO tokens_revocados (jti_hash, id_usuario, fecha_revocacion, expira_en)
            VALUES ({jtiHash}, {userId}, {now}, {DateTime.SpecifyKind(parsedToken.ValidTo, DateTimeKind.Unspecified)})
            ON CONFLICT (jti_hash) DO NOTHING
            """);

        return NoContent();
    }
}
