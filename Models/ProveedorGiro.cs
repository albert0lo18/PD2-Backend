using System;
using System.Collections.Generic;

namespace PD2_Backend.Models;

public partial class ProveedorGiro
{
    public int IdProveedor { get; set; }

    public int IdGiro { get; set; }

    public string? ProductoServicio { get; set; }

    public virtual Giro IdGiroNavigation { get; set; } = null!;

    public virtual Proveedore IdProveedorNavigation { get; set; } = null!;
}
