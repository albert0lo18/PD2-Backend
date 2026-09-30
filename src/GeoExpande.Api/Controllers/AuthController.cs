using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeoExpande.Api.Dtos;
using GeoExpande.Api.Models;
using GeoExpande.Api.Services;

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
        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            return BadRequest("La contraseña debe tener al menos 6 caracteres.");

        var correoExiste = await _context.Usuarios.AnyAsync(u => u.Correo == dto.Correo);
        if (correoExiste)
            return Conflict("Ya existe un usuario con ese correo.");

        var defaultRoleName = _config["Auth:DefaultRoleName"] ?? "Usuario";
        var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == defaultRoleName);
        if (rol is null)
            return StatusCode(500, $"No existe el rol por defecto '{defaultRoleName}' en la tabla roles.");

        var usuario = new Usuario
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Correo = dto.Correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IdRol = rol.IdRol,
            IdEmpresa = dto.IdEmpresa,
            Activo = true
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

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

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto dto)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.Correo == dto.Correo);

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
}