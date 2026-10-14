namespace GeoExpande.Api.Dtos;

public class RecomendacionZonaDto
{
    public int IdEstado { get; set; }
    public int IdMunicipio { get; set; }
    public string NombreZona { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public int? Poblacion { get; set; }
    public long CompetidoresCercanos { get; set; }
    public long NegociosCercanosTotal { get; set; }
    public decimal PuntuacionCompetencia { get; set; }
    public decimal PuntuacionPoblacion { get; set; }
    public decimal PuntuacionAccesibilidad { get; set; }
    public decimal Puntuacion { get; set; }
    public decimal CuotaPresenciaEstimada { get; set; }
    public decimal DemandaEstimadaMensualMxn { get; set; }
    public string NivelRiesgo { get; set; } = string.Empty;
    public string Recomendacion { get; set; } = string.Empty;
}
