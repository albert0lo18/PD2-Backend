using System;
using System.Collections.Generic;

namespace GeoExpande.Api.Models;

public partial class Analisi
{
    public int IdAnalisis { get; set; }

    public int IdEmpresa { get; set; }

    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFinalizacion { get; set; }

    public string Estado { get; set; } = null!;

    public virtual ICollection<EscenariosExpansion> EscenariosExpansions { get; set; } = new List<EscenariosExpansion>();

    public virtual Empresa IdEmpresaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
