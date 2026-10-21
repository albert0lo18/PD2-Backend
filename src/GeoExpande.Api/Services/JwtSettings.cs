using System.Text;

namespace GeoExpande.Api.Services;

public sealed class JwtSettings
{
    private JwtSettings(string key, string issuer, string audience, int expiresMinutes)
    {
        Key = key;
        Issuer = issuer;
        Audience = audience;
        ExpiresMinutes = expiresMinutes;
    }

    public string Key { get; }
    public string Issuer { get; }
    public string Audience { get; }
    public int ExpiresMinutes { get; }

    public static JwtSettings FromConfiguration(IConfiguration configuration)
    {
        var key = configuration["Jwt:Key"];
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException("Jwt:Key debe configurarse con al menos 32 bytes para HS256.");
        if (string.IsNullOrWhiteSpace(issuer))
            throw new InvalidOperationException("Falta configurar Jwt:Issuer.");
        if (string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("Falta configurar Jwt:Audience.");

        var expiresValue = configuration["Jwt:ExpiresMinutes"];
        if (!int.TryParse(expiresValue, out var expiresMinutes)
            || expiresMinutes is < 5 or > 1440)
        {
            throw new InvalidOperationException("Jwt:ExpiresMinutes debe ser un entero entre 5 y 1440.");
        }

        return new JwtSettings(key, issuer, audience, expiresMinutes);
    }
}
