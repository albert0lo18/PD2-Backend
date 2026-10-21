namespace GeoExpande.Api.Dtos;

public class DashboardResumenDto
{
    public int AnalisisRealizados { get; set; }
    public int ZonasEvaluadas { get; set; }
    public int ZonasAltoPotencial { get; set; }
    public int EstablecimientosGiro { get; set; }
    public DateOnly? UltimaActualizacionDenue { get; set; }
    public List<DashboardMetricaDto> Metricas { get; set; } = [];
    public List<ComparativaCiudadDto> ComparativaCiudades { get; set; } = [];
}

public class DashboardMetricaDto
{
    public string Etiqueta { get; set; } = string.Empty;
    public int Valor { get; set; }
    public string Delta { get; set; } = string.Empty;
}

public class ComparativaCiudadDto
{
    public string Ciudad { get; set; } = string.Empty;
    public decimal Puntuacion { get; set; }
    public int EstablecimientosGiro { get; set; }
}
