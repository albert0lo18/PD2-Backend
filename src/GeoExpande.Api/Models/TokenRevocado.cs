namespace GeoExpande.Api.Models;

public class TokenRevocado
{
    public string JtiHash { get; set; } = null!;
    public int IdUsuario { get; set; }
    public DateTime FechaRevocacion { get; set; }
    public DateTime ExpiraEn { get; set; }
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
