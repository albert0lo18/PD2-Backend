using System;
using System.Collections.Generic;

namespace GeoExpande.Api.Models;

public partial class Empresa
{
    public int IdEmpresa { get; set; }

    public string NombreComercial { get; set; } = null!;

    public string? RazonSocial { get; set; }

    public string? Rfc { get; set; }

    public int? IdSector { get; set; }

    public int? IdGiro { get; set; }

    public string? DescripcionNegocio { get; set; }

    public string? CiudadBase { get; set; }

    public string? Tamano { get; set; }

    public string? Telefono { get; set; }

    public string? Correo { get; set; }

    public string? SitioWeb { get; set; }

    public DateTime FechaRegistro { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Analisi> Analisis { get; set; } = new List<Analisi>();

    public virtual Giro? IdGiroNavigation { get; set; }

    public virtual Sectore? IdSectorNavigation { get; set; }

    public virtual ICollection<PreferenciasExpansion> PreferenciasExpansions { get; set; } = new List<PreferenciasExpansion>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
