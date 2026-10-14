namespace GeoExpande.Api.Dtos;

public class RecomendacionGuardadaDto
{
    public int IdUbicacion { get; set; }
    public int IdEstado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int IdMunicipio { get; set; }
    public string Municipio { get; set; } = string.Empty;
    public int? IdLocalidad { get; set; }
    public string? NombreUbicacion { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public decimal Puntuacion { get; set; }
    public decimal? PuntuacionCompetencia { get; set; }
    public decimal? PuntuacionPoblacion { get; set; }
    public decimal? PuntuacionProveedores { get; set; }
    public decimal? PuntuacionAccesibilidad { get; set; }
    public int? CompetidoresCercanos { get; set; }
    public int? ProveedoresCercanos { get; set; }
    public decimal? CuotaPresenciaEstimada { get; set; }
    public decimal? DemandaEstimadaMensualMxn { get; set; }
    public string? NivelRiesgo { get; set; }
    public string? Recomendacion { get; set; }
    public int? Ranking { get; set; }
    public DateTime FechaCalculo { get; set; }
}
