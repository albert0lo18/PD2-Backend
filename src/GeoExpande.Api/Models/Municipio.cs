using System;
using System.Collections.Generic;

namespace GeoExpande.Api.Models;

public partial class Municipio
{
    public int IdMunicipio { get; set; }

    public int IdEstado { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public int? Poblacion { get; set; }

    public virtual ICollection<EstablecimientosDenue> EstablecimientosDenues { get; set; } = new List<EstablecimientosDenue>();

    public virtual Estado IdEstadoNavigation { get; set; } = null!;

    public virtual ICollection<Localidade> Localidades { get; set; } = new List<Localidade>();

    public virtual ICollection<UbicacionesRecomendada> UbicacionesRecomendada { get; set; } = new List<UbicacionesRecomendada>();
}
