using System;
using System.Collections.Generic;

namespace PD2_Backend.Models;

public partial class Proveedore
{
    public int IdProveedor { get; set; }

    public string NombreComercial { get; set; } = null!;

    public string? RazonSocial { get; set; }

    public string? Rfc { get; set; }

    public string? Descripcion { get; set; }

    public string? Telefono { get; set; }

    public string? Correo { get; set; }

    public string? SitioWeb { get; set; }

    public string? Calle { get; set; }

    public string? NumeroExterior { get; set; }

    public string? Colonia { get; set; }

    public string? CodigoPostal { get; set; }

    public int? IdEstado { get; set; }

    public int? IdMunicipio { get; set; }

    public int? IdLocalidad { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual Estado? IdEstadoNavigation { get; set; }

    public virtual Localidade? IdLocalidadNavigation { get; set; }

    public virtual Municipio? IdMunicipioNavigation { get; set; }

    public virtual ICollection<ProveedorGiro> ProveedorGiros { get; set; } = new List<ProveedorGiro>();

    public virtual ICollection<ProveedorSector> ProveedorSectors { get; set; } = new List<ProveedorSector>();

    public virtual ICollection<ProveedoresRecomendado> ProveedoresRecomendados { get; set; } = new List<ProveedoresRecomendado>();
}
