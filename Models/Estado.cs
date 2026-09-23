using System;
using System.Collections.Generic;

namespace PD2_Backend.Models;

public partial class Estado
{
    public int IdEstado { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public virtual ICollection<EstablecimientosDenue> EstablecimientosDenues { get; set; } = new List<EstablecimientosDenue>();

    public virtual ICollection<Municipio> Municipios { get; set; } = new List<Municipio>();

    public virtual ICollection<Proveedore> Proveedores { get; set; } = new List<Proveedore>();

    public virtual ICollection<UbicacionesRecomendada> UbicacionesRecomendada { get; set; } = new List<UbicacionesRecomendada>();
}
