using System;
using System.Collections.Generic;

namespace GeoExpande.Api.Models;

public partial class EscenariosExpansion
{
    public int IdEscenario { get; set; }

    public int IdAnalisis { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal? Presupuesto { get; set; }

    public int? NumeroUnidades { get; set; }

    public decimal? RadioBusquedaKm { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual Analisi IdAnalisisNavigation { get; set; } = null!;

    public virtual ICollection<UbicacionesRecomendada> UbicacionesRecomendada { get; set; } = new List<UbicacionesRecomendada>();
}
