using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GeoExpande.Api.Models;
using GeoExpande.Api.Services;
using Microsoft.Extensions.Configuration;

namespace GeoExpande.Tests;

[TestClass]
public class AutenticacionTests
{
    private static IConfiguration Config(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values.ToDictionary(x => x.Key, x => (string?)x.Value)).Build();

    [TestMethod]
    public void JwtSettings_AceptaConfiguracionValida()
    {
        var settings = JwtSettings.FromConfiguration(Config(("Jwt:Key", new string('k', 32)), ("Jwt:Issuer", "GeoExpande"), ("Jwt:Audience", "web"), ("Jwt:ExpiresMinutes", "60")));
        Assert.AreEqual(60, settings.ExpiresMinutes);
    }

    [TestMethod]
    public void JwtSettings_RechazaClaveMenorA32Bytes()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => JwtSettings.FromConfiguration(Config(("Jwt:Key", new string('k', 31)), ("Jwt:Issuer", "i"), ("Jwt:Audience", "a"), ("Jwt:ExpiresMinutes", "60"))));
        StringAssert.Contains(ex.Message, "32 bytes");
    }

    [TestMethod]
    public void JwtSettings_RechazaExpiracionFueraDeRango()
    {
        Assert.Throws<InvalidOperationException>(() => JwtSettings.FromConfiguration(Config(("Jwt:Key", new string('k', 32)), ("Jwt:Issuer", "i"), ("Jwt:Audience", "a"), ("Jwt:ExpiresMinutes", "4"))));
    }

    [TestMethod]
    public void TokenService_EmiteTokenConIdentidadYRol()
    {
        var service = new TokenService(new JwtSettingsForTest().Settings);
        var user = new Usuario { IdUsuario = 42, Nombre = "Ana", Correo = "ana@example.com", IdEmpresa = 7 };
        var (token, expiry) = service.GenerateToken(user, "Admin");
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.AreEqual("42", jwt.Subject);
        Assert.AreEqual("Admin", jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        Assert.AreEqual("7", jwt.Claims.First(c => c.Type == "id_empresa").Value);
        Assert.IsTrue(expiry > DateTime.UtcNow);
    }

    [TestMethod]
    public void HashTokenId_EsDeterministaYNoExponeElIdOriginal()
    {
        var first = TokenRevocationService.HashTokenId("jti-ejemplo");
        Assert.AreEqual(first, TokenRevocationService.HashTokenId("jti-ejemplo"));
        Assert.AreEqual(64, first.Length);
        Assert.AreNotEqual("jti-ejemplo", first);
    }

    private sealed class JwtSettingsForTest
    {
        public JwtSettings Settings { get; } = JwtSettings.FromConfiguration(Config(("Jwt:Key", new string('k', 32)), ("Jwt:Issuer", "GeoExpande"), ("Jwt:Audience", "web"), ("Jwt:ExpiresMinutes", "60")));
    }
}
