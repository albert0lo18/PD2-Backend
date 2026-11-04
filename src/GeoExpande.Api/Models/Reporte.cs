namespace GeoExpande.Api.Models;

public class Reporte
{
    public int IdReporte { get; set; }
    public int IdAnalisis { get; set; }
    public int IdUsuario { get; set; }
    public bool IncluirMapas { get; set; }
    public bool IncluirProveedores { get; set; }
    public DateTime FechaGeneracion { get; set; }
    public string NombreArchivo { get; set; } = null!;
    public byte[] ArchivoPdf { get; set; } = null!;

    public virtual Analisi IdAnalisisNavigation { get; set; } = null!;
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
