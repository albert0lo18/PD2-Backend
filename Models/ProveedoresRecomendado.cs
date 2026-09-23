using System;
using System.Collections.Generic;

namespace PD2_Backend.Models;

public partial class ProveedoresRecomendado
{
    public int IdRecomendacion { get; set; }

    public int IdUbicacion { get; set; }

    public int IdProveedor { get; set; }

    public decimal? DistanciaKm { get; set; }

    public decimal? Puntuacion { get; set; }

    public string? Motivo { get; set; }

    public int? Ranking { get; set; }

    public DateTime FechaCalculo { get; set; }

    public virtual Proveedore IdProveedorNavigation { get; set; } = null!;

    public virtual UbicacionesRecomendada IdUbicacionNavigation { get; set; } = null!;
}
