using System;
using System.Collections.Generic;

namespace GeoExpande.Api.Models;

public partial class EstablecimientosDenue
{
    public long IdEstablecimiento { get; set; }

    public int IdEstado { get; set; }

    public int IdMunicipio { get; set; }

    public int? IdLocalidad { get; set; }

    public string? Nombre { get; set; }

    public string? RazonSocial { get; set; }

    public string? CodigoActividad { get; set; }

    public string? ActividadEconomica { get; set; }

    public string? Sector { get; set; }

    public int? IdSector { get; set; }

    public int? IdGiro { get; set; }

    public string? Calle { get; set; }

    public string? NumeroExterior { get; set; }

    public string? NumeroInterior { get; set; }

    public string? Colonia { get; set; }

    public string? CodigoPostal { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    public string? TipoVialidad { get; set; }

    public string? CorredorIndustrial { get; set; }

    public string? TipoEstablecimiento { get; set; }

    public DateOnly? FechaActualizacion { get; set; }

    public string? Clee { get; set; }

    public virtual Estado IdEstadoNavigation { get; set; } = null!;

    public virtual Giro? IdGiroNavigation { get; set; }

    public virtual Localidade? IdLocalidadNavigation { get; set; }

    public virtual Municipio IdMunicipioNavigation { get; set; } = null!;

    public virtual Sectore? IdSectorNavigation { get; set; }
}
