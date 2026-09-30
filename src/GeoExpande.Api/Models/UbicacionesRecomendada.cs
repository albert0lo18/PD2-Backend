using System;
using System.Collections.Generic;

namespace GeoExpande.Api.Models;

public partial class UbicacionesRecomendada
{
    public int IdUbicacion { get; set; }

    public int IdEscenario { get; set; }

    public int IdEstado { get; set; }

    public int IdMunicipio { get; set; }

    public int? IdLocalidad { get; set; }

    public string? NombreUbicacion { get; set; }

    public decimal Puntuacion { get; set; }

    public decimal? PuntuacionCompetencia { get; set; }

    public decimal? PuntuacionPoblacion { get; set; }

    public decimal? PuntuacionProveedores { get; set; }

    public decimal? PuntuacionAccesibilidad { get; set; }

    public int? CompetidoresCercanos { get; set; }

    public int? ProveedoresCercanos { get; set; }

    public decimal? CuotaPresenciaEstimada { get; set; }

    public decimal? DemandaEstimadaMensualMxn { get; set; }

    public string? NivelRiesgo { get; set; }

    public string? Recomendacion { get; set; }

    public int? Ranking { get; set; }

    public DateTime FechaCalculo { get; set; }

    public virtual EscenariosExpansion IdEscenarioNavigation { get; set; } = null!;

    public virtual Estado IdEstadoNavigation { get; set; } = null!;

    public virtual Localidade? IdLocalidadNavigation { get; set; }

    public virtual Municipio IdMunicipioNavigation { get; set; } = null!;
}
