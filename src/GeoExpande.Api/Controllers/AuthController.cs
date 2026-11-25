using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeoExpande.Api.Dtos;
using GeoExpande.Api.Models;
using GeoExpande.Api.Services;
using System.Security.Cryptography;

namespace GeoExpande.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly GeoExpandeDbContext _context;
    private readonly TokenService _tokenService;
    private readonly IConfiguration _config;
    private readonly EmailVerificationService _emailVerificationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(GeoExpandeDbContext context, TokenService tokenService, IConfiguration config,
        EmailVerificationService emailVerificationService, ILogger<AuthController> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _config = config;
        _emailVerificationService = emailVerificationService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponseDto>> Register(RegisterRequestDto dto)
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
        dto.SitioWeb = string.IsNullOrWhiteSpace(dto.SitioWeb) ? null : dto.SitioWeb.Trim();

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
                SitioWeb = dto.SitioWeb,
                Activo = false
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
                Activo = false
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            var codigo = EmailVerificationService.CreateCode();
            _context.EmailVerificationCodes.Add(new EmailVerificationCode
            {
                IdUsuario = usuario.IdUsuario,
                CodigoHash = EmailVerificationService.HashCode(codigo),
                CreadoEn = DateTime.UtcNow,
                ExpiraEn = DateTime.UtcNow.AddMinutes(10),
                Intentos = 0
            });
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            try
            {
                await _emailVerificationService.SendCodeAsync(usuario.Correo, usuario.Nombre, codigo, HttpContext.RequestAborted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo enviar el código de verificación a {Correo}", usuario.Correo);
                var savedVerification = await _context.EmailVerificationCodes.FindAsync(usuario.IdUsuario);
                if (savedVerification is not null)
                {
                    savedVerification.CreadoEn = DateTime.UtcNow.AddSeconds(-61);
                    await _context.SaveChangesAsync();
                }
                return Accepted(new RegisterResponseDto
                {
                    Correo = usuario.Correo,
                    Mensaje = "La cuenta quedó pendiente, pero no pudimos enviar el correo. Revisa la configuración del servidor o intenta reenviar la clave.",
                    CorreoEnviado = false
                });
            }

            return Accepted(new RegisterResponseDto
            {
                Correo = usuario.Correo,
                Mensaje = "Enviamos una clave de verificación a tu correo.",
                CorreoEnviado = true
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequestDto dto)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios
            .Include(user => user.IdEmpresaNavigation)
            .FirstOrDefaultAsync(user => user.Correo.ToLower() == correo);
        if (usuario is null || usuario.Activo)
            return BadRequest("No encontramos una verificación pendiente para ese correo.");

        var verification = await _context.EmailVerificationCodes.FindAsync(usuario.IdUsuario);
        if (verification is null || verification.ExpiraEn <= DateTime.UtcNow)
            return BadRequest("La clave venció. Solicita una nueva clave para continuar.");
        if (verification.Intentos >= 5)
            return BadRequest("Se agotaron los intentos. Solicita una nueva clave para continuar.");

        verification.Intentos++;
        var providedHash = Convert.FromHexString(EmailVerificationService.HashCode(dto.Codigo));
        var expectedHash = Convert.FromHexString(verification.CodigoHash);
        if (!CryptographicOperations.FixedTimeEquals(providedHash, expectedHash))
        {
            await _context.SaveChangesAsync();
            return BadRequest("La clave no es correcta.");
        }

        usuario.Activo = true;
        if (usuario.IdEmpresaNavigation is not null)
            usuario.IdEmpresaNavigation.Activo = true;
        _context.EmailVerificationCodes.Remove(verification);
        await _context.SaveChangesAsync();
        return Ok(new RegisterResponseDto { Correo = usuario.Correo, Mensaje = "Correo verificado. Ya puedes iniciar sesión." });
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification(ResendVerificationRequestDto dto)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(user => user.Correo.ToLower() == correo && !user.Activo);
        if (usuario is null)
            return Accepted(new { mensaje = "Si hay una verificación pendiente para ese correo, enviaremos una nueva clave." });

        var verification = await _context.EmailVerificationCodes.FindAsync(usuario.IdUsuario);
        if (verification is null)
            return Accepted(new { mensaje = "Si hay una verificación pendiente para ese correo, enviaremos una nueva clave." });
        if (verification.CreadoEn > DateTime.UtcNow.AddSeconds(-60))
            return Accepted(new { mensaje = "Espera un minuto antes de solicitar otra clave." });

        var code = EmailVerificationService.CreateCode();
        verification.CodigoHash = EmailVerificationService.HashCode(code);
        verification.CreadoEn = DateTime.UtcNow;
        verification.ExpiraEn = DateTime.UtcNow.AddMinutes(10);
        verification.Intentos = 0;
        await _context.SaveChangesAsync();
        try
        {
            await _emailVerificationService.SendCodeAsync(usuario.Correo, usuario.Nombre, code, HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo reenviar el código de verificación a {Correo}", usuario.Correo);
            verification.CreadoEn = DateTime.UtcNow.AddSeconds(-61);
            await _context.SaveChangesAsync();
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "No pudimos enviar el correo. Inténtalo de nuevo más tarde.");
        }
        return Accepted(new { mensaje = "Enviamos una nueva clave de verificación." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto dto)
    {
        const string responseMessage = "Si existe una cuenta activa con ese correo, enviaremos un código para restablecer la contraseña.";
        var correo = dto.Correo.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(user => user.Correo.ToLower() == correo && user.Activo);
        if (usuario is null)
            return Accepted(new { mensaje = responseMessage });

        var resetCode = await _context.PasswordResetCodes.FindAsync(usuario.IdUsuario);
        if (resetCode is not null && resetCode.CreadoEn > DateTime.UtcNow.AddSeconds(-60))
            return Accepted(new { mensaje = responseMessage });

        var code = EmailVerificationService.CreateCode();
        if (resetCode is null)
        {
            resetCode = new PasswordResetCode { IdUsuario = usuario.IdUsuario };
            _context.PasswordResetCodes.Add(resetCode);
        }
        resetCode.CodigoHash = EmailVerificationService.HashCode(code);
        resetCode.CreadoEn = DateTime.UtcNow;
        resetCode.ExpiraEn = DateTime.UtcNow.AddMinutes(10);
        resetCode.Intentos = 0;
        await _context.SaveChangesAsync();

        try
        {
            await _emailVerificationService.SendPasswordResetCodeAsync(usuario.Correo, usuario.Nombre, code, HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo enviar el código de recuperación a {Correo}", usuario.Correo);
            _context.PasswordResetCodes.Remove(resetCode);
            await _context.SaveChangesAsync();
        }

        return Accepted(new { mensaje = responseMessage });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto dto)
    {
        if (Encoding.UTF8.GetByteCount(dto.Password) > 72)
            return BadRequest("La contraseña no puede superar 72 bytes en UTF-8.");

        var correo = dto.Correo.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(user => user.Correo.ToLower() == correo && user.Activo);
        if (usuario is null)
            return BadRequest("El código no es válido o venció. Solicita uno nuevo.");

        var resetCode = await _context.PasswordResetCodes.FindAsync(usuario.IdUsuario);
        if (resetCode is null || resetCode.ExpiraEn <= DateTime.UtcNow)
            return BadRequest("El código no es válido o venció. Solicita uno nuevo.");
        if (resetCode.Intentos >= 5)
            return BadRequest("Se agotaron los intentos. Solicita un código nuevo.");

        resetCode.Intentos++;
        var providedHash = Convert.FromHexString(EmailVerificationService.HashCode(dto.Codigo));
        var expectedHash = Convert.FromHexString(resetCode.CodigoHash);
        if (!CryptographicOperations.FixedTimeEquals(providedHash, expectedHash))
        {
            await _context.SaveChangesAsync();
            return BadRequest("El código no es correcto.");
        }

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        _context.PasswordResetCodes.Remove(resetCode);
        await _context.SaveChangesAsync();
        return Ok(new { mensaje = "Tu contraseña se cambió correctamente. Ya puedes iniciar sesión." });
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

        if (usuario is null)
            return Unauthorized("Correo o contraseña incorrectos.");

        var passwordValida = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);
        if (!passwordValida)
            return Unauthorized("Correo o contraseña incorrectos.");

        if (!usuario.Activo)
        {
            var pendiente = await _context.EmailVerificationCodes.AnyAsync(code => code.IdUsuario == usuario.IdUsuario);
            if (pendiente)
                return BadRequest("Verifica tu correo con la clave de 4 dígitos antes de iniciar sesión.");
            return Unauthorized("Correo o contraseña incorrectos.");
        }

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

        var now = DateTime.UtcNow;
        await _context.TokensRevocados
            .Where(token => token.ExpiraEn <= now)
            .ExecuteDeleteAsync();

        var jtiHash = TokenRevocationService.HashTokenId(tokenId);
        await _context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO tokens_revocados (jti_hash, id_usuario, fecha_revocacion, expira_en)
            VALUES ({jtiHash}, {userId}, {now}, {DateTime.SpecifyKind(parsedToken.ValidTo, DateTimeKind.Utc)})
            ON CONFLICT (jti_hash) DO NOTHING
            """);

        return NoContent();
    }
}
