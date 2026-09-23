using System;
using System.Collections.Generic;

namespace PD2_Backend.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public int? IdEmpresa { get; set; }

    public int IdRol { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Apellido { get; set; }

    public string Correo { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Analisi> Analisis { get; set; } = new List<Analisi>();

    public virtual Empresa? IdEmpresaNavigation { get; set; }

    public virtual Role IdRolNavigation { get; set; } = null!;

    public virtual ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();
}
