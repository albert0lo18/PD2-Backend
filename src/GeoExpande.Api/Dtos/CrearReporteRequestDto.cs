using System.ComponentModel.DataAnnotations;

namespace GeoExpande.Api.Dtos;

public class CrearReporteRequestDto
{
    [Range(1, int.MaxValue)]
    public int IdAnalisis { get; set; }

    public bool IncluirMapas { get; set; }

    public bool IncluirProveedores { get; set; }

    [StringLength(4_000_000)]
    public string? MapaJpegBase64 { get; set; }

    [Range(1, 2000)]
    public int? MapaAncho { get; set; }

    [Range(1, 2000)]
    public int? MapaAlto { get; set; }
}
