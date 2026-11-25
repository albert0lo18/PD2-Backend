namespace GeoExpande.Api.Models;

public sealed class PasswordResetCode
{
    public int IdUsuario { get; set; }
    public string CodigoHash { get; set; } = null!;
    public DateTime ExpiraEn { get; set; }
    public DateTime CreadoEn { get; set; }
    public int Intentos { get; set; }
    public Usuario Usuario { get; set; } = null!;
}
