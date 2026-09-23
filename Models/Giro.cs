using System;
using System.Collections.Generic;

namespace PD2_Backend.Models;

public partial class Giro
{
    public int IdGiro { get; set; }

    public int IdSector { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Empresa> Empresas { get; set; } = new List<Empresa>();

    public virtual Sectore IdSectorNavigation { get; set; } = null!;

    public virtual ICollection<ProveedorGiro> ProveedorGiros { get; set; } = new List<ProveedorGiro>();
}
