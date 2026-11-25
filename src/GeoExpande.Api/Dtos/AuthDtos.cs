using System.ComponentModel.DataAnnotations;

namespace GeoExpande.Api.Dtos;

public sealed class OptionalUrlAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is null || value is string text && string.IsNullOrWhiteSpace(text))
            return true;
        return new UrlAttribute().IsValid(value);
    }
}

public class RegisterRequestDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Nombre { get; set; } = null!;

    [StringLength(100)]
    public string? Apellido { get; set; }

    [Required, EmailAddress, StringLength(150)]
    public string Correo { get; set; } = null!;

    [Required, StringLength(72, MinimumLength = 12)]
    public string Password { get; set; } = null!;

    [StringLength(250)]
    public string? RazonSocial { get; set; }

    [Required, StringLength(200, MinimumLength = 1)]
    public string NombreComercial { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int IdSector { get; set; }

    [Range(1, int.MaxValue)]
    public int IdGiro { get; set; }

    [StringLength(1000)]
    public string? DescripcionNegocio { get; set; }

    [StringLength(150)]
    public string? CiudadBase { get; set; }

    [StringLength(20)]
    [RegularExpression(@"^[0-9+().\-\s]*$", ErrorMessage = "El teléfono contiene caracteres no válidos.")]
    public string? Telefono { get; set; }

    [StringLength(250)]
    [OptionalUrl]
    public string? SitioWeb { get; set; }
}

public class LoginRequestDto
{
    [Required, EmailAddress, StringLength(150)]
    public string Correo { get; set; } = null!;

    [Required, StringLength(72, MinimumLength = 1)]
    public string Password { get; set; } = null!;
}

public class VerifyEmailRequestDto
{
    [Required, EmailAddress, StringLength(150)]
    public string Correo { get; set; } = null!;

    [Required, RegularExpression(@"^\d{4}$", ErrorMessage = "La clave debe tener 4 dígitos.")]
    public string Codigo { get; set; } = null!;
}

public class ResendVerificationRequestDto
{
    [Required, EmailAddress, StringLength(150)]
    public string Correo { get; set; } = null!;
}

public class ForgotPasswordRequestDto
{
    [Required, EmailAddress, StringLength(150)]
    public string Correo { get; set; } = null!;
}

public class ResetPasswordRequestDto
{
    [Required, EmailAddress, StringLength(150)]
    public string Correo { get; set; } = null!;

    [Required, RegularExpression(@"^\d{4}$", ErrorMessage = "El código debe tener 4 dígitos.")]
    public string Codigo { get; set; } = null!;

    [Required, StringLength(72, MinimumLength = 12)]
    public string Password { get; set; } = null!;
}

public class RegisterResponseDto
{
    public string Correo { get; set; } = null!;
    public string Mensaje { get; set; } = null!;
    public bool CorreoEnviado { get; set; }
}

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string Rol { get; set; } = null!;
}
