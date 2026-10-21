using System.Security.Cryptography;
using System.Text;

namespace GeoExpande.Api.Services;

public static class TokenRevocationService
{
    public static string HashTokenId(string tokenId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(tokenId));
        return Convert.ToHexString(hash);
    }
}
