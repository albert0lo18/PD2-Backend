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
[Route("api/analisis")]
public class AnalisisController : ControllerBase
{
    private readonly GeoExpandeDbContext _context;

    public AnalisisController(GeoExpandeDbContext context) => _context = context;

    [HttpGet("historial")]
    public async Task<IActionResult> GetHistorial(
        [FromQuery, Range(1, int.MaxValue)] int? page = null,
        [FromQuery, Range(1, 100)] int? pageSize = null)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized();

        var query = _context.Analises
            .AsNoTracking()
            .Where(a => a.IdUsuario == userId);
        var historialQuery = query
            .OrderByDescending(a => a.FechaInicio)
            .ThenByDescending(a => a.IdAnalisis)
            .Select(a => new
            {
                id = a.IdAnalisis,
                nombre = a.Nombre,
                giro = a.IdEmpresaNavigation.IdGiroNavigation == null
                    ? null
                    : a.IdEmpresaNavigation.IdGiroNavigation.Nombre,
                fechaCreacion = a.FechaInicio,
                estado = a.Estado,
                zonasEvaluadas = a.EscenariosExpansions
                    .SelectMany(escenario => escenario.UbicacionesRecomendada)
                    .Select(zona => zona.IdMunicipio)
                    .Distinct()
                    .Count()
            });

        // El cliente actual consume una lista cuando no solicita paginación.
        // Se conserva la respuesta paginada para llamadas que envían page o pageSize.
        if (!page.HasValue && !pageSize.HasValue)
            return Ok(await historialQuery.ToListAsync());

        var pagina = page ?? 1;
        var tamanoPagina = pageSize ?? 10;
        var totalItems = await query.CountAsync();
        var skipCount = (int)Math.Min((long)(pagina - 1) * tamanoPagina, int.MaxValue);
        var historial = await historialQuery
            .Skip(skipCount)
            .Take(tamanoPagina)
            .ToListAsync();

        return Ok(new
        {
            page = pagina,
            pageSize = tamanoPagina,
            totalItems,
            totalPages = (int)Math.Ceiling(totalItems / (double)tamanoPagina),
            items = historial
        });
    }

    [HttpGet("{analisisId:int}/zonas")]
    public async Task<IActionResult> GetZonas(
        int analisisId,
        [FromQuery] string nivel = "municipal",
        [FromQuery] int? estadoId = null,
        [FromQuery] int? municipioId = null,
        [FromQuery] double? minLat = null,
        [FromQuery] double? maxLat = null,
        [FromQuery] double? minLng = null,
        [FromQuery] double? maxLng = null)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized();

        nivel = nivel.Trim().ToLowerInvariant();
        if (nivel is not ("municipal" or "estatal" or "nacional"))
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Nivel geográfico no válido",
                detail: "El parámetro nivel admite 'municipal', 'estatal' o 'nacional'.");
        if (!GeographicBoundsValidator.TryValidate(minLat, maxLat, minLng, maxLng, out var boundsError))
            return BadRequest(boundsError);

        var analisisExiste = await _context.Analises
            .AsNoTracking()
            .AnyAsync(a => a.IdAnalisis == analisisId && a.IdUsuario == userId);
        if (!analisisExiste)
            return NotFound("No se encontró el análisis solicitado.");

        var query = _context.UbicacionesRecomendadas
            .AsNoTracking()
            .Where(z => z.IdEscenarioNavigation.IdAnalisis == analisisId);

        if (estadoId.HasValue)
            query = query.Where(z => z.IdEstado == estadoId.Value);
        if (municipioId.HasValue)
            query = query.Where(z => z.IdMunicipio == municipioId.Value);

        var zonas = await query
            .OrderBy(z => z.Ranking ?? int.MaxValue)
            .ThenByDescending(z => z.Puntuacion)
            .Take(500)
            .Select(z => new
            {
                idUbicacion = z.IdUbicacion,
                idEstado = z.IdEstado,
                estado = z.IdEstadoNavigation.Nombre,
                idMunicipio = z.IdMunicipio,
                municipio = z.IdMunicipioNavigation.Nombre,
                puntaje = z.Puntuacion
            })
            .ToListAsync();

        var ubicacionIds = zonas.Select(zona => zona.idUbicacion).ToArray();
        var coordenadas = await _context.Database
            .SqlQuery<UbicacionCoordenadasDto>($"""
                SELECT
                    z.id_ubicacion AS "IdUbicacion",
                    ST_Y(z.ubicacion::geometry) AS "Lat",
                    ST_X(z.ubicacion::geometry) AS "Lng"
                FROM ubicaciones_recomendadas z
                JOIN escenarios_expansion escenario ON escenario.id_escenario = z.id_escenario
                WHERE escenario.id_analisis = {analisisId}
                    AND z.id_ubicacion = ANY({ubicacionIds})
                    AND ({minLat} IS NULL OR (ST_Y(z.ubicacion::geometry) BETWEEN {minLat} AND {maxLat}
                        AND ST_X(z.ubicacion::geometry) BETWEEN {minLng} AND {maxLng}))
                """)
            .ToDictionaryAsync(punto => punto.IdUbicacion);

        var zonasConCoordenadas = zonas.Where(zona => coordenadas.ContainsKey(zona.idUbicacion)).Select(zona => new
        {
            zona.idUbicacion,
            zona.idEstado,
            zona.estado,
            zona.idMunicipio,
            zona.municipio,
            puntaje = zona.puntaje,
            latitud = coordenadas[zona.idUbicacion].Lat,
            longitud = coordenadas[zona.idUbicacion].Lng
        }).Where(zona => double.IsFinite(zona.latitud) && double.IsFinite(zona.longitud)
            && zona.latitud is >= -90 and <= 90 && zona.longitud is >= -180 and <= 180).ToList();

        if (nivel == "municipal")
            return Ok(zonasConCoordenadas);

        var agrupadas = nivel == "estatal"
            ? zonasConCoordenadas.GroupBy(zona => new { zona.idEstado, zona.estado })
                .Select(grupo => new
                {
                    nivel,
                    idEstado = (int?)grupo.Key.idEstado,
                    estado = (string?)grupo.Key.estado,
                    idMunicipio = (int?)null,
                    municipio = (string?)null,
                    puntaje = grupo.Max(zona => zona.puntaje),
                    latitud = grupo.Average(zona => zona.latitud),
                    longitud = grupo.Average(zona => zona.longitud),
                    zonasAgrupadas = grupo.Count()
                }).ToList()
            : zonasConCoordenadas
                .Select(zona => new
                {
                    nivel,
                    zona.idEstado,
                    zona.estado,
                    zona.idMunicipio,
                    zona.municipio,
                    zona.puntaje,
                    zona.latitud,
                    zona.longitud,
                    zonasAgrupadas = 1
                })
                .GroupBy(_ => 1)
                .Select(grupo => new
                {
                    nivel,
                    idEstado = (int?)null,
                    estado = (string?)null,
                    idMunicipio = (int?)null,
                    municipio = (string?)null,
                    puntaje = grupo.Max(zona => zona.puntaje),
                    latitud = grupo.Average(zona => zona.latitud),
                    longitud = grupo.Average(zona => zona.longitud),
                    zonasAgrupadas = grupo.Sum(zona => zona.zonasAgrupadas)
                }).ToList();

        return Ok(agrupadas);
    }

    [HttpGet("{analisisId:int}/puntos-calor")]
    public async Task<IActionResult> GetPuntosCalor(
        int analisisId,
        [FromQuery] string nivel = "municipal",
        [FromQuery] int? municipioId = null,
        [FromQuery] double? minLat = null,
        [FromQuery] double? maxLat = null,
        [FromQuery] double? minLng = null,
        [FromQuery] double? maxLng = null,
        [FromQuery, Range(0, 22)] int zoom = 10)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized();

        nivel = nivel.Trim().ToLowerInvariant();
        if (nivel is not ("municipal" or "estatal" or "nacional"))
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Nivel geográfico no válido",
                detail: "El parámetro nivel admite 'municipal', 'estatal' o 'nacional'.");
        if (!GeographicBoundsValidator.TryValidate(minLat, maxLat, minLng, maxLng, out var boundsError))
            return BadRequest(boundsError);

        var analisisExiste = await _context.Analises
            .AsNoTracking()
            .AnyAsync(a => a.IdAnalisis == analisisId && a.IdUsuario == userId);
        if (!analisisExiste)
            return NotFound("No se encontró el análisis solicitado.");

        var zonasQuery = _context.UbicacionesRecomendadas
            .AsNoTracking()
            .Where(z => z.IdEscenarioNavigation.IdAnalisis == analisisId);

        if (municipioId.HasValue)
            zonasQuery = zonasQuery.Where(z => z.IdMunicipio == municipioId.Value);

        var puntuacionesPorMunicipio = await zonasQuery
            .GroupBy(z => z.IdMunicipio)
            .Select(grupo => new
            {
                IdMunicipio = grupo.Key,
                Puntuacion = grupo.Max(z => z.Puntuacion)
            })
            .ToDictionaryAsync(z => z.IdMunicipio, z => z.Puntuacion);

        var municipioIds = puntuacionesPorMunicipio.Keys.ToList();
        if (municipioIds.Count == 0)
            return Ok(Array.Empty<object>());

        var idGiroEmpresa = await _context.Analises
            .AsNoTracking()
            .Where(analisis => analisis.IdAnalisis == analisisId && analisis.IdUsuario == userId)
            .Select(analisis => analisis.IdEmpresaNavigation.IdGiro)
            .FirstOrDefaultAsync();

        if (idGiroEmpresa is not int giroEmpresa)
            return Ok(Array.Empty<object>());

        var establecimientos = await _context.EstablecimientosDenues
            .AsNoTracking()
            .Where(e => municipioIds.Contains(e.IdMunicipio)
                && e.IdGiro.HasValue
                && e.IdGiro.Value == giroEmpresa
                && e.Latitud.HasValue
                && e.Longitud.HasValue
                && e.Latitud >= -90
                && e.Latitud <= 90
                && e.Longitud >= -180
                && e.Longitud <= 180
                && (!minLat.HasValue || (e.Latitud >= (decimal)minLat.Value && e.Latitud <= (decimal)maxLat!.Value
                    && e.Longitud >= (decimal)minLng!.Value && e.Longitud <= (decimal)maxLng!.Value)))
            .OrderBy(e => e.IdEstablecimiento)
            .Select(e => new
            {
                e.IdMunicipio,
                latitud = (double)e.Latitud!.Value,
                longitud = (double)e.Longitud!.Value
            })
            .Take(5001)
            .ToListAsync();

        if (establecimientos.Count > 5000)
            establecimientos = establecimientos.Take(5000).ToList();

        var cellSize = Math.Pow(2, 12 - zoom) / 1000d;
        var puntos = establecimientos.Select(establecimiento => new
        {
            latitud = Math.Round(establecimiento.latitud / cellSize) * cellSize,
            longitud = Math.Round(establecimiento.longitud / cellSize) * cellSize,
            intensidad = Math.Clamp(
                (double)(puntuacionesPorMunicipio[establecimiento.IdMunicipio] / 100m),
                0d,
                1d)
        })
            .GroupBy(punto => (punto.latitud, punto.longitud))
            .Select(grupo => new
            {
                latitud = grupo.Average(punto => punto.latitud),
                longitud = grupo.Average(punto => punto.longitud),
                intensidad = grupo.Max(punto => punto.intensidad),
                cantidad = grupo.Count()
            });

        // El cliente existente espera una lista directa de puntos.
        return Ok(puntos.ToList());
    }

    private bool TryGetAuthenticatedUserId(out int userId)
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out userId);
    }
}
