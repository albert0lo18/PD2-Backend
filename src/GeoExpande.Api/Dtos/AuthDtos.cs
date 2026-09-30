namespace GeoExpande.Api.Dtos;

public class RegisterRequestDto
{
    public string Nombre { get; set; } = null!;
    public string? Apellido { get; set; }
    public string Correo { get; set; } = null!;
    public string Password { get; set; } = null!;
    public int? IdEmpresa { get; set; }
}

public class LoginRequestDto
{
    public string Correo { get; set; } = null!;
    public string Password { get; set; } = null!;
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