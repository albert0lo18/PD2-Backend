using System;
using System.Collections.Generic;

namespace PD2_Backend.Models;

public partial class ProveedorSector
{
    public int IdProveedor { get; set; }

    public int IdSector { get; set; }

    public bool EsPrincipal { get; set; }

    public virtual Proveedore IdProveedorNavigation { get; set; } = null!;

    public virtual Sectore IdSectorNavigation { get; set; } = null!;
}
