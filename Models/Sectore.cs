using System;
using System.Collections.Generic;

namespace PD2_Backend.Models;

public partial class Sectore
{
    public int IdSector { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Empresa> Empresas { get; set; } = new List<Empresa>();

    public virtual ICollection<Giro> Giros { get; set; } = new List<Giro>();

    public virtual ICollection<ProveedorSector> ProveedorSectors { get; set; } = new List<ProveedorSector>();
}
