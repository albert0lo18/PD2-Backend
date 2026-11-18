using System.IdentityModel.Tokens.Jwt;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using GeoExpande.Api.Dtos;
using GeoExpande.Api.Models;
using GeoExpande.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoExpande.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly GeoExpandeDbContext _context;
    private readonly ReportePdfBuilder _pdfBuilder;

    public ReportesController(GeoExpandeDbContext context, ReportePdfBuilder pdfBuilder)
    {
        _context = context;
        _pdfBuilder = pdfBuilder;
    }

    [HttpPost("ejecutivo")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GenerarEjecutivo(CrearReporteRequestDto request)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized();

        var analisis = await _context.Analises
            .AsNoTracking()
            .Where(item => item.IdAnalisis == request.IdAnalisis && item.IdUsuario == userId)
            .Select(item => new
            {
                item.IdAnalisis,
                item.Nombre,
                Giro = item.IdEmpresaNavigation.IdGiroNavigation == null
                    ? "Giro sin especificar"
                    : item.IdEmpresaNavigation.IdGiroNavigation.Nombre
            })
            .FirstOrDefaultAsync();

        if (analisis is null)
            return NotFound("No se encontró el análisis solicitado.");

        byte[]? mapaJpeg = null;
        if (!string.IsNullOrWhiteSpace(request.MapaJpegBase64))
        {
            if (!request.IncluirMapas)
                return BadRequest("La imagen del mapa solo se admite cuando IncluirMapas está activado.");
            if (!request.MapaAncho.HasValue || !request.MapaAlto.HasValue)
                return BadRequest("La imagen del mapa debe incluir ancho y alto.");

            try
            {
                mapaJpeg = Convert.FromBase64String(request.MapaJpegBase64);
            }
            catch (FormatException)
            {
                return BadRequest("La imagen del mapa no tiene un formato Base64 válido.");
            }

            if (mapaJpeg.Length < 4 || mapaJpeg[0] != 0xFF || mapaJpeg[1] != 0xD8
                || mapaJpeg[^2] != 0xFF || mapaJpeg[^1] != 0xD9)
                return BadRequest("La imagen del mapa debe ser un JPEG válido.");
        }

        var zonas = await _context.Database.SqlQuery<ReporteZonaQueryDto>($"""
            SELECT
                COALESCE(z.ranking, 2147483647) AS "Ranking",
                m.nombre AS "Municipio",
                e.nombre AS "Estado",
                z.puntuacion AS "Puntuacion",
                z.competidores_cercanos AS "CompetidoresCercanos",
                z.demanda_estimada_mensual_mxn AS "DemandaEstimadaMensualMxn",
                z.cuota_presencia_estimada AS "CuotaPresenciaEstimada",
                z.nivel_riesgo AS "NivelRiesgo",
                z.recomendacion AS "Recomendacion",
                z.proveedores_cercanos AS "ProveedoresCercanos",
                z.puntuacion_proveedores AS "PuntuacionProveedores",
                ST_Y(z.ubicacion::geometry) AS "Latitud",
                ST_X(z.ubicacion::geometry) AS "Longitud"
            FROM ubicaciones_recomendadas z
            JOIN escenarios_expansion escenario ON escenario.id_escenario = z.id_escenario
            JOIN municipios m ON m.id_municipio = z.id_municipio
            JOIN estados e ON e.id_estado = z.id_estado
            WHERE escenario.id_analisis = {analisis.IdAnalisis}
            ORDER BY z.ranking NULLS LAST, z.puntuacion DESC
            """).ToListAsync();

        var fechaGeneracion = DateTime.UtcNow;
        var nombreArchivo = $"GeoExpande-Reporte-{analisis.IdAnalisis}-{fechaGeneracion:yyyyMMdd-HHmmss}.pdf";
        var archivoPdf = _pdfBuilder.Build(new ReportePdfDatos(
            analisis.Nombre,
            analisis.Giro,
            fechaGeneracion,
            request.IncluirMapas,
            request.IncluirProveedores,
            zonas.Select(zona => new ReportePdfZona(
                zona.Ranking,
                zona.Municipio,
                zona.Estado,
                zona.Puntuacion,
                zona.CompetidoresCercanos,
                zona.DemandaEstimadaMensualMxn,
                zona.CuotaPresenciaEstimada,
                zona.NivelRiesgo,
                zona.Recomendacion,
                zona.ProveedoresCercanos,
                zona.PuntuacionProveedores,
                zona.Latitud,
                zona.Longitud)).ToList(),
            mapaJpeg,
            request.MapaAncho,
            request.MapaAlto));

        var reporte = new Reporte
        {
            IdAnalisis = analisis.IdAnalisis,
            IdUsuario = userId,
            IncluirMapas = request.IncluirMapas,
            IncluirProveedores = request.IncluirProveedores,
            FechaGeneracion = fechaGeneracion,
            NombreArchivo = nombreArchivo,
            ArchivoPdf = archivoPdf
        };

        _context.Reportes.Add(reporte);
        await _context.SaveChangesAsync();

        Response.Headers["X-Reporte-Id"] = reporte.IdReporte.ToString();
        Response.Headers["X-Generado-En"] = fechaGeneracion.ToString("O");
        return File(archivoPdf, "application/pdf", nombreArchivo);
    }

    [HttpGet("historial")]
    public async Task<ActionResult<ReporteHistorialResponseDto>> GetHistorial(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized();

        var query = _context.Reportes
            .AsNoTracking()
            .Where(reporte => reporte.IdUsuario == userId);

        var totalItems = await query.CountAsync();
        var skipCount = (int)Math.Min((long)(page - 1) * pageSize, int.MaxValue);
        var items = await query
            .OrderByDescending(reporte => reporte.FechaGeneracion)
            .ThenByDescending(reporte => reporte.IdReporte)
            .Skip(skipCount)
            .Take(pageSize)
            .Select(reporte => new ReporteHistorialItemDto
            {
                IdReporte = reporte.IdReporte,
                IdAnalisis = reporte.IdAnalisis,
                NombreAnalisis = reporte.IdAnalisisNavigation.Nombre,
                FechaGeneracion = reporte.FechaGeneracion,
                IncluirMapas = reporte.IncluirMapas,
                IncluirProveedores = reporte.IncluirProveedores,
                NombreArchivo = reporte.NombreArchivo
            })
            .ToListAsync();

        return Ok(new ReporteHistorialResponseDto
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            Items = items
        });
    }

    [HttpGet("{idReporte:int}/archivo")]
    [Produces("application/pdf")]
    public async Task<IActionResult> DescargarArchivo(int idReporte)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized();

        var reporte = await _context.Reportes
            .AsNoTracking()
            .Where(item => item.IdReporte == idReporte && item.IdUsuario == userId)
            .Select(item => new { item.ArchivoPdf, item.NombreArchivo })
            .FirstOrDefaultAsync();

        return reporte is null
            ? NotFound("No se encontró el reporte solicitado.")
            : File(reporte.ArchivoPdf, "application/pdf", reporte.NombreArchivo);
    }

    private bool TryGetAuthenticatedUserId(out int userId)
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out userId);
    }
}
