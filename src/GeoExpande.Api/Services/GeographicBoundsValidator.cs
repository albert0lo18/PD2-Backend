namespace GeoExpande.Api.Services;

/// <summary>Valida los límites geográficos opcionales usados por consultas de análisis.</summary>
public static class GeographicBoundsValidator
{
    public static bool TryValidate(
        double? minLat, double? maxLat, double? minLng, double? maxLng, out string? error)
    {
        var suppliedCount = new[] { minLat, maxLat, minLng, maxLng }.Count(value => value.HasValue);
        if (suppliedCount is not (0 or 4))
        {
            error = "Debe proporcionar juntos minLat, maxLat, minLng y maxLng.";
            return false;
        }
        if (suppliedCount == 4 && (!double.IsFinite(minLat!.Value) || !double.IsFinite(maxLat!.Value)
            || !double.IsFinite(minLng!.Value) || !double.IsFinite(maxLng!.Value)
            || minLat < -90 || maxLat > 90 || minLng < -180 || maxLng > 180
            || minLat >= maxLat || minLng >= maxLng))
        {
            error = "El cuadro geográfico debe tener coordenadas válidas y min menor que max.";
            return false;
        }
        error = null;
        return true;
    }
}
