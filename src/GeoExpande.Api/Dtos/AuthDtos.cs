namespace GeoExpande.Api.Dtos;

public class RegisterRequestDto
{
    public string Nombre { get; set; } = null!;
    public string Apellido { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string RazonSocial { get; set; } = null!;
    public string NombreComercial { get; set; } = null!;
    public int IdSector { get; set; }
    public int IdGiro { get; set; }
    public string DescripcionNegocio { get; set; } = null!;
    public string CiudadBase { get; set; } = null!;
    public string Telefono { get; set; } = null!;
    public string SitioWeb { get; set; } = null!;
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
