using System.Globalization;
using System.Text;

namespace GeoExpande.Api.Services;

public sealed record ReportePdfZona(
    int Ranking,
    string Municipio,
    string Estado,
    decimal Puntuacion,
    int? CompetidoresCercanos,
    decimal? DemandaEstimadaMensualMxn,
    decimal? CuotaPresenciaEstimada,
    string? NivelRiesgo,
    string? Recomendacion,
    int? ProveedoresCercanos,
    decimal? PuntuacionProveedores,
    double? Latitud,
    double? Longitud);

public sealed record ReportePdfDatos(
    string NombreAnalisis,
    string Giro,
    DateTime FechaGeneracion,
    bool IncluirMapas,
    bool IncluirProveedores,
    IReadOnlyList<ReportePdfZona> Zonas);

/// <summary>
/// Genera un PDF sencillo con fuentes estándar, evitando depender de archivos
/// locales o de un servicio externo para la generación del reporte.
/// </summary>
public sealed class ReportePdfBuilder
{
    private const double PageWidth = 595;
    private const double PageHeight = 842;
    private static readonly CultureInfo EsMx = CultureInfo.GetCultureInfo("es-MX");

    public byte[] Build(ReportePdfDatos datos)
    {
        var lineas = CrearContenido(datos);
        var paginas = DividirEnPaginas(lineas, 52);
        if (datos.IncluirMapas)
            paginas.Add(CrearPaginaMapa(datos.Zonas));

        return CrearDocumentoPdf(paginas);
    }

    private static List<string> CrearContenido(ReportePdfDatos datos)
    {
        var lineas = new List<string>
        {
            "GEOEXPANDE | REPORTE EJECUTIVO",
            $"Analisis: {datos.NombreAnalisis}",
            $"Giro: {datos.Giro}",
            $"Generado (UTC): {datos.FechaGeneracion.ToString("dd/MM/yyyy HH:mm", EsMx)}",
            $"Zonas incluidas: {datos.Zonas.Count}",
            string.Empty,
            "RESUMEN DE OPORTUNIDADES"
        };

        if (datos.Zonas.Count == 0)
        {
            lineas.Add("El analisis no contiene zonas recomendadas.");
        }

        foreach (var zona in datos.Zonas)
        {
            lineas.Add(string.Empty);
            lineas.Add($"{zona.Ranking}. {zona.Municipio}, {zona.Estado} | Puntuacion: {zona.Puntuacion:0.##}/100");
            lineas.Add($"   Competidores cercanos: {zona.CompetidoresCercanos?.ToString(EsMx) ?? "Sin dato"}");
            lineas.Add($"   Demanda mensual estimada: {FormatoMoneda(zona.DemandaEstimadaMensualMxn)}");
            lineas.Add($"   Cuota de presencia estimada: {FormatoPorcentaje(zona.CuotaPresenciaEstimada)}");
            lineas.Add($"   Nivel de riesgo: {zona.NivelRiesgo ?? "Sin dato"}");
            if (!string.IsNullOrWhiteSpace(zona.Recomendacion))
                lineas.Add($"   Recomendacion: {zona.Recomendacion}");

            if (datos.IncluirProveedores)
            {
                lineas.Add($"   Proveedores cercanos estimados: {zona.ProveedoresCercanos?.ToString(EsMx) ?? "Sin dato"}");
                lineas.Add($"   Puntuacion de proveedores: {(zona.PuntuacionProveedores.HasValue ? $"{zona.PuntuacionProveedores.Value.ToString("0.##", EsMx)}/100" : "Sin dato")}");
            }
        }

        if (datos.IncluirMapas)
        {
            lineas.Add(string.Empty);
            lineas.Add("El mapa esquematico de ubicaciones se incluye en la pagina siguiente.");
        }

        lineas.Add(string.Empty);
        lineas.Add("Las puntuaciones, estimaciones y recomendaciones provienen del analisis guardado en GeoExpande.");
        return lineas.SelectMany(EnvolverLinea).ToList();
    }

    private static IEnumerable<string> EnvolverLinea(string linea)
    {
        const int maxChars = 92;
        if (string.IsNullOrEmpty(linea))
        {
            yield return string.Empty;
            yield break;
        }

        var restante = linea;
        while (restante.Length > maxChars)
        {
            var corte = restante.LastIndexOf(' ', maxChars);
            if (corte <= 0)
                corte = maxChars;
            yield return restante[..corte];
            restante = "   " + restante[(corte + 1)..].TrimStart();
        }

        yield return restante;
    }

    private static List<List<string>> DividirEnPaginas(IReadOnlyList<string> lineas, int lineasPorPagina)
    {
        var paginas = new List<List<string>>();
        for (var offset = 0; offset < lineas.Count; offset += lineasPorPagina)
            paginas.Add(lineas.Skip(offset).Take(lineasPorPagina).ToList());
        if (paginas.Count == 0)
            paginas.Add([]);
        return paginas;
    }

    private static List<string> CrearPaginaMapa(IReadOnlyList<ReportePdfZona> zonas)
    {
        var lineas = new List<string>
        {
            "MAPA ESQUEMATICO DE UBICACIONES",
            "Los puntos muestran coordenadas de las zonas recomendadas; no es un mapa base."
        };
        lineas.AddRange(zonas.Select(zona =>
            $"{zona.Ranking}. {zona.Municipio}, {zona.Estado} | {zona.Latitud?.ToString("0.0000", EsMx) ?? "sin latitud"}, {zona.Longitud?.ToString("0.0000", EsMx) ?? "sin longitud"}"));
        return lineas;
    }

    private static string FormatoMoneda(decimal? value) => value.HasValue
        ? value.Value.ToString("C0", EsMx)
        : "Sin dato";

    private static string FormatoPorcentaje(decimal? value) => value.HasValue
        ? $"{value.Value:0.##}%"
        : "Sin dato";

    private static byte[] CrearDocumentoPdf(IReadOnlyList<List<string>> paginas)
    {
        var objetos = new List<byte[]> { Array.Empty<byte>(), Array.Empty<byte>(), Array.Empty<byte>() };
        var idsPagina = new List<int>();
        var idsContenido = new List<int>();

        for (var i = 0; i < paginas.Count; i++)
        {
            idsPagina.Add(objetos.Count + 1);
            objetos.Add(Array.Empty<byte>());
            idsContenido.Add(objetos.Count + 1);
            objetos.Add(Array.Empty<byte>());
        }

        objetos[0] = Ascii($"<< /Type /Catalog /Pages 2 0 R >>");
        objetos[1] = Ascii($"<< /Type /Pages /Kids [{string.Join(" ", idsPagina.Select(id => $"{id} 0 R"))}] /Count {paginas.Count} >>");
        objetos[2] = Ascii("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");

        for (var i = 0; i < paginas.Count; i++)
        {
            var contenido = i == paginas.Count - 1 && paginas.Count > 1
                && paginas[i].FirstOrDefault()?.StartsWith("MAPA ESQUEMATICO", StringComparison.Ordinal) == true
                ? CrearContenidoMapa(paginas[i])
                : CrearContenidoTexto(paginas[i], i + 1, paginas.Count);

            objetos[idsPagina[i] - 1] = Ascii(
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PageWidth:0} {PageHeight:0}] /Resources << /Font << /F1 3 0 R >> >> /Contents {idsContenido[i]} 0 R >>");
            objetos[idsContenido[i] - 1] = CrearObjetoFlujo(contenido);
        }

        using var salida = new MemoryStream();
        Escribir(salida, Ascii("%PDF-1.4\n% GeoExpande\n"));
        var offsets = new List<long> { 0 };
        for (var i = 0; i < objetos.Count; i++)
        {
            offsets.Add(salida.Position);
            Escribir(salida, Ascii($"{i + 1} 0 obj\n"));
            Escribir(salida, objetos[i]);
            Escribir(salida, Ascii("\nendobj\n"));
        }

        var inicioXref = salida.Position;
        Escribir(salida, Ascii($"xref\n0 {objetos.Count + 1}\n0000000000 65535 f \n"));
        for (var i = 1; i < offsets.Count; i++)
            Escribir(salida, Ascii($"{offsets[i]:0000000000} 00000 n \n"));
        Escribir(salida, Ascii($"trailer\n<< /Size {objetos.Count + 1} /Root 1 0 R >>\nstartxref\n{inicioXref}\n%%EOF"));
        return salida.ToArray();
    }

    private static byte[] CrearContenidoTexto(IReadOnlyList<string> lineas, int pagina, int totalPaginas)
    {
        var sb = new StringBuilder();
        var y = 792;
        foreach (var linea in lineas)
        {
            var tamano = linea.StartsWith("GEOEXPANDE", StringComparison.Ordinal) ? 16 : 10;
            sb.AppendLine($"BT /F1 {tamano} Tf 48 {y} Td ({PdfString(linea)}) Tj ET");
            y -= tamano == 16 ? 26 : 14;
        }
        sb.AppendLine($"BT /F1 8 Tf 48 32 Td (GeoExpande - pagina {pagina} de {totalPaginas}) Tj ET");
        return Ascii(sb.ToString());
    }

    private static byte[] CrearContenidoMapa(IReadOnlyList<string> lineas)
    {
        var sb = new StringBuilder();
        sb.AppendLine("BT /F1 15 Tf 48 800 Td (MAPA ESQUEMATICO DE UBICACIONES) Tj ET");
        sb.AppendLine("BT /F1 9 Tf 48 782 Td (Vista de coordenadas de las zonas; no incluye mapa base.) Tj ET");

        const double left = 78;
        const double bottom = 175;
        const double width = 438;
        const double height = 540;
        sb.AppendLine("0.25 w");
        sb.AppendLine($"{left} {bottom} {width} {height} re S");

        var zonas = lineas.Skip(2)
            .Select(ParsearLineaMapa)
            .Where(zona => zona is not null)
            .Select(zona => zona!)
            .ToList();
        var conCoordenadas = zonas.Where(zona => zona.Latitud.HasValue && zona.Longitud.HasValue).ToList();

        if (conCoordenadas.Count == 0)
        {
            sb.AppendLine("BT /F1 11 Tf 135 440 Td (No hay coordenadas disponibles para graficar.) Tj ET");
        }
        else
        {
            var minLat = conCoordenadas.Min(zona => zona.Latitud!.Value);
            var maxLat = conCoordenadas.Max(zona => zona.Latitud!.Value);
            var minLng = conCoordenadas.Min(zona => zona.Longitud!.Value);
            var maxLng = conCoordenadas.Max(zona => zona.Longitud!.Value);
            if (Math.Abs(maxLat - minLat) < 0.001) { minLat -= 0.01; maxLat += 0.01; }
            if (Math.Abs(maxLng - minLng) < 0.001) { minLng -= 0.01; maxLng += 0.01; }

            sb.AppendLine("0.75 w");
            for (var i = 1; i < 5; i++)
            {
                var x = left + width * i / 5;
                var y = bottom + height * i / 5;
                sb.AppendLine($"{x:0.##} {bottom} m {x:0.##} {bottom + height} l S");
                sb.AppendLine($"{left} {y:0.##} m {left + width} {y:0.##} l S");
            }

            foreach (var zona in conCoordenadas)
            {
                var x = left + (zona.Longitud!.Value - minLng) / (maxLng - minLng) * width;
                var y = bottom + (zona.Latitud!.Value - minLat) / (maxLat - minLat) * height;
                sb.AppendLine($"0.12 0.35 0.72 rg {x - 3:0.##} {y - 3:0.##} 6 6 re f 0 0 0 rg");
                sb.AppendLine($"BT /F1 8 Tf {Math.Min(x + 5, left + width - 70):0.##} {Math.Min(y + 5, bottom + height - 10):0.##} Td ({PdfString($"{zona.Ranking}. {zona.Municipio}")}) Tj ET");
            }
        }

        sb.AppendLine("BT /F1 8 Tf 48 32 Td (GeoExpande - mapa esquematico de ubicaciones recomendadas) Tj ET");
        return Ascii(sb.ToString());
    }

    private static ZonaMapa? ParsearLineaMapa(string linea)
    {
        // Formato: ranking. Municipio, Estado | latitud, longitud
        var partes = linea.Split(" | ", StringSplitOptions.TrimEntries);
        if (partes.Length != 2)
            return null;
        var coordenadas = partes[1].Split(',', StringSplitOptions.TrimEntries);
        if (coordenadas.Length != 2)
            return null;

        var cabecera = partes[0].Split('.', 2, StringSplitOptions.TrimEntries);
        if (cabecera.Length != 2)
            return null;
        var lugar = cabecera[1].Split(',', 2, StringSplitOptions.TrimEntries);
        if (lugar.Length == 0)
            return null;

        var latitud = double.TryParse(coordenadas[0], NumberStyles.Float, EsMx, out var lat) ? lat : (double?)null;
        var longitud = double.TryParse(coordenadas[1], NumberStyles.Float, EsMx, out var lng) ? lng : (double?)null;
        return new ZonaMapa(cabecera[0], lugar[0], latitud, longitud);
    }

    private static string PdfString(string value)
    {
        var normalizado = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var caracter in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) == UnicodeCategory.NonSpacingMark)
                continue;
            var ascii = caracter switch
            {
                '–' or '—' => '-',
                '€' => 'E',
                '’' or '‘' => '\'',
                '“' or '”' => '"',
                _ => caracter
            };
            if (ascii > 127)
                ascii = '?';
            if (ascii is '(' or ')' or '\\')
                sb.Append('\\');
            if (ascii is '\r' or '\n')
                sb.Append(' ');
            else
                sb.Append(ascii);
        }
        return sb.ToString();
    }

    private static byte[] CrearObjetoFlujo(byte[] contenido)
    {
        using var salida = new MemoryStream();
        Escribir(salida, Ascii($"<< /Length {contenido.Length} >>\nstream\n"));
        Escribir(salida, contenido);
        Escribir(salida, Ascii("endstream"));
        return salida.ToArray();
    }

    private static byte[] Ascii(string value) => Encoding.ASCII.GetBytes(value);

    private static void Escribir(Stream salida, byte[] bytes) => salida.Write(bytes, 0, bytes.Length);

    private sealed record ZonaMapa(string Ranking, string Municipio, double? Latitud, double? Longitud);
}
