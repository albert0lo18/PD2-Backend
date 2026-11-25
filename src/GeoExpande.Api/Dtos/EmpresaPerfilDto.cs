namespace GeoExpande.Api.Dtos;

public class EmpresaPerfilDto
{
    public string Correo { get; set; } = null!;
    public string RazonSocial { get; set; } = null!;
    public string NombreComercial { get; set; } = null!;
    public string CiudadBase { get; set; } = null!;
    public int IdSector { get; set; }
    public int IdGiro { get; set; }
    public string? Tamano { get; set; }
    public string DescripcionNegocio { get; set; } = null!;
}
