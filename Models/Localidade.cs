using System;
using System.Collections.Generic;

namespace PD2_Backend.Models;

public partial class Localidade
{
    public int IdLocalidad { get; set; }

    public int IdMunicipio { get; set; }

    public string? Clave { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<EstablecimientosDenue> EstablecimientosDenues { get; set; } = new List<EstablecimientosDenue>();

    public virtual Municipio IdMunicipioNavigation { get; set; } = null!;

    public virtual ICollection<Proveedore> Proveedores { get; set; } = new List<Proveedore>();

    public virtual ICollection<UbicacionesRecomendada> UbicacionesRecomendada { get; set; } = new List<UbicacionesRecomendada>();
}
