namespace GeoExpande.Api.Dtos;

public class ReporteHistorialItemDto
{
    public int IdReporte { get; set; }
    public int IdAnalisis { get; set; }
    public string NombreAnalisis { get; set; } = string.Empty;
    public DateTime FechaGeneracion { get; set; }
    public bool IncluirMapas { get; set; }
    public bool IncluirProveedores { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
}

public class ReporteHistorialResponseDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public List<ReporteHistorialItemDto> Items { get; set; } = [];
}

public class ReporteZonaQueryDto
{
    public int Ranking { get; set; }
    public string Municipio { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal Puntuacion { get; set; }
    public int? CompetidoresCercanos { get; set; }
    public decimal? DemandaEstimadaMensualMxn { get; set; }
    public decimal? CuotaPresenciaEstimada { get; set; }
    public string? NivelRiesgo { get; set; }
    public string? Recomendacion { get; set; }
    public int? ProveedoresCercanos { get; set; }
    public decimal? PuntuacionProveedores { get; set; }
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }
}
