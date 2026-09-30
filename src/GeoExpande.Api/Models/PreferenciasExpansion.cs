using System;
using System.Collections.Generic;

namespace GeoExpande.Api.Models;

public partial class PreferenciasExpansion
{
    public int IdPreferencia { get; set; }

    public int IdEmpresa { get; set; }

    public decimal? PresupuestoMinimo { get; set; }

    public decimal? PresupuestoMaximo { get; set; }

    public decimal? RadioAnalisisKm { get; set; }

    public int? NumeroUnidadesDeseadas { get; set; }

    public string? RegionesInteresTexto { get; set; }

    public decimal? PrioridadCompetencia { get; set; }

    public decimal? PrioridadPoblacion { get; set; }

    public decimal? PrioridadProveedores { get; set; }

    public decimal? PrioridadAccesibilidad { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public virtual Empresa IdEmpresaNavigation { get; set; } = null!;
}
