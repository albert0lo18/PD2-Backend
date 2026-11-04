using System.ComponentModel.DataAnnotations;

namespace GeoExpande.Api.Dtos;

public class CrearReporteRequestDto
{
    [Range(1, int.MaxValue)]
    public int IdAnalisis { get; set; }

    public bool IncluirMapas { get; set; }

    public bool IncluirProveedores { get; set; }
}
