using System.Text;
using GeoExpande.Api.Services;

namespace GeoExpande.Tests;

[TestClass]
public class ReportesTests
{
    private static ReportePdfDatos Datos(params ReportePdfZona[] zonas) =>
        new("Análisis de prueba", "Cafetería", new DateTime(2026, 1, 2, 3, 4, 0, DateTimeKind.Utc), false, false, zonas);

    [TestMethod]
    public void Build_GeneraDocumentoConFirmaPdf()
    {
        var pdf = new ReportePdfBuilder().Build(Datos());
        Assert.IsTrue(Encoding.ASCII.GetString(pdf).StartsWith("%PDF-1.4"));
    }

    [TestMethod]
    public void Build_IncluyeDatosPrincipalesDelAnalisis()
    {
        var pdf = Encoding.ASCII.GetString(new ReportePdfBuilder().Build(Datos()));
        StringAssert.Contains(pdf, "Analisis: Analisis de prueba");
        StringAssert.Contains(pdf, "Giro: Cafeteria");
        StringAssert.Contains(pdf, "Zonas incluidas: 0");
    }

    [TestMethod]
    public void Build_IndicaCuandoNoHayZonas()
    {
        var pdf = Encoding.ASCII.GetString(new ReportePdfBuilder().Build(Datos()));
        StringAssert.Contains(pdf, "no contiene zonas recomendadas");
    }

    [TestMethod]
    public void Build_IncluyeZonaYMetricas()
    {
        var zona = new ReportePdfZona(1, "Hermosillo", "Sonora", 85.5m, 3, 12000m, 0.25m, "bajo", "Buena oportunidad", null, null, null, null);
        var pdf = Encoding.ASCII.GetString(new ReportePdfBuilder().Build(Datos(zona)));
        StringAssert.Contains(pdf, "Hermosillo, Sonora");
        StringAssert.Contains(pdf, "Buena oportunidad");
        StringAssert.Contains(pdf, "85.5/100");
    }

    [TestMethod]
    public void Build_AnexaPaginaDeMapaCuandoSeSolicita()
    {
        var zona = new ReportePdfZona(1, "Hermosillo", "Sonora", 85m, null, null, null, null, null, null, null, 29.07, -110.96);
        var datos = Datos(zona) with { IncluirMapas = true, IncluirProveedores = true };
        var pdf = Encoding.ASCII.GetString(new ReportePdfBuilder().Build(datos));
        StringAssert.Contains(pdf, "MAPA ESQUEMATICO DE UBICACIONES");
        StringAssert.Contains(pdf, "Proveedores cercanos estimados");
    }
}
