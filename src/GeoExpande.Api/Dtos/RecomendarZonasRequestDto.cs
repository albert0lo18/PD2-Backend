using System.ComponentModel.DataAnnotations;

namespace GeoExpande.Api.Dtos;

public class RecomendarZonasRequestDto
{
    [Range(1, int.MaxValue)]
    public int IdGiro { get; set; }

    [Range(typeof(decimal), "0.1", "500")]
    public decimal? RadioKm { get; set; }
}
