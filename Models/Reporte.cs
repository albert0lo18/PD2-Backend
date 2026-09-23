using System;
using System.Collections.Generic;

namespace PD2_Backend.Models;

public partial class Reporte
{
    public int IdReporte { get; set; }

    public int IdAnalisis { get; set; }

    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string? RutaArchivo { get; set; }

    public DateTime FechaGeneracion { get; set; }

    public virtual Analisi IdAnalisisNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
