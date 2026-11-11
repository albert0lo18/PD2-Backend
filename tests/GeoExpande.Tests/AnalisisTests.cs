using GeoExpande.Api.Services;

namespace GeoExpande.Tests;

[TestClass]
public class AnalisisTests
{
    [TestMethod]
    public void Limites_OpcionalesVaciosSonValidos() => AssertValid(null, null, null, null);

    [TestMethod]
    public void Limites_CompletosConCoordenadasValidasSonAceptados() => AssertValid(28.0, 29.0, -112.0, -111.0);

    [TestMethod]
    public void Limites_RechazaParametrosParciales()
    {
        Assert.IsFalse(GeographicBoundsValidator.TryValidate(28, 29, null, null, out var error));
        StringAssert.Contains(error, "juntos");
    }

    [TestMethod]
    public void Limites_RechazaCoordenadaFueraDeRango()
    {
        Assert.IsFalse(GeographicBoundsValidator.TryValidate(-91, 29, -112, -111, out var error));
        StringAssert.Contains(error, "coordenadas válidas");
    }

    [TestMethod]
    public void Limites_RechazaRangoDegradadoEnFrontera()
    {
        Assert.IsFalse(GeographicBoundsValidator.TryValidate(28, 28, -112, -111, out _));
        Assert.IsFalse(GeographicBoundsValidator.TryValidate(28, 29, -112, -112, out _));
    }

    private static void AssertValid(double? a, double? b, double? c, double? d)
    {
        Assert.IsTrue(GeographicBoundsValidator.TryValidate(a, b, c, d, out var error), error);
        Assert.IsNull(error);
    }
}
