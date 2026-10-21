using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GeoExpande.Api.Dtos;

public class PreferenciasExpansionDto
{
    [Range(typeof(decimal), "0", "9999999999999999")]
    [JsonPropertyName("presupuestoMinimoMXN")]
    public decimal? PresupuestoMinimoMxn { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999")]
    [JsonPropertyName("presupuestoMaximoMXN")]
    public decimal? PresupuestoMaximoMxn { get; set; }

    [Range(typeof(decimal), "0.1", "500")]
    public decimal? RadioBusquedaKm { get; set; }

    [Range(1, int.MaxValue)]
    public int? NumeroUnidadesDeseadas { get; set; }

    public List<string> RegionesInteres { get; set; } = [];

    [Range(typeof(decimal), "0", "100")]
    public decimal? PrioridadCompetencia { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal? PrioridadPoblacion { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal? PrioridadProveedores { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal? PrioridadAccesibilidad { get; set; }
}
