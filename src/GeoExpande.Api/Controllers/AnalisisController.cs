using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GeoExpande.Api.Dtos;
using GeoExpande.Api.Models;
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
    public async Task<IActionResult> GetHistorial()
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized();

        var historial = await _context.Analises
            .AsNoTracking()
            .Where(a => a.IdUsuario == userId)
            .OrderByDescending(a => a.FechaInicio)
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
            })
            .ToListAsync();

        return Ok(historial);
    }

    [HttpGet("{analisisId:int}/zonas")]
    public async Task<IActionResult> GetZonas(
        int analisisId,
        [FromQuery] string nivel = "municipal",
        [FromQuery] int? estadoId = null,
        [FromQuery] int? municipioId = null)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized();

        if (!string.Equals(nivel, "municipal", StringComparison.OrdinalIgnoreCase))
            return BadRequest("El parámetro nivel solo admite el valor 'municipal'.");

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

        var coordenadas = await _context.Database
            .SqlQuery<UbicacionCoordenadasDto>($"""
                SELECT
                    z.id_ubicacion AS "IdUbicacion",
                    ST_Y(z.ubicacion::geometry) AS "Lat",
                    ST_X(z.ubicacion::geometry) AS "Lng"
                FROM ubicaciones_recomendadas z
                JOIN escenarios_expansion escenario ON escenario.id_escenario = z.id_escenario
                WHERE escenario.id_analisis = {analisisId}
                """)
            .ToDictionaryAsync(punto => punto.IdUbicacion);

        return Ok(zonas.Select(zona => new
        {
            zona.idUbicacion,
            zona.idEstado,
            zona.estado,
            zona.idMunicipio,
            zona.municipio,
            puntaje = zona.puntaje,
            latitud = coordenadas[zona.idUbicacion].Lat,
            longitud = coordenadas[zona.idUbicacion].Lng
        }));
    }

    [HttpGet("{analisisId:int}/puntos-calor")]
    public async Task<IActionResult> GetPuntosCalor(
        int analisisId,
        [FromQuery] string nivel = "municipal",
        [FromQuery] int? municipioId = null)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized();

        if (!string.Equals(nivel, "municipal", StringComparison.OrdinalIgnoreCase))
            return BadRequest("El parámetro nivel solo admite el valor 'municipal'.");

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
                && e.Longitud <= 180)
            .Select(e => new
            {
                e.IdMunicipio,
                latitud = (double)e.Latitud!.Value,
                longitud = (double)e.Longitud!.Value
            })
            .ToListAsync();

        var puntos = establecimientos.Select(establecimiento => new
        {
            establecimiento.latitud,
            establecimiento.longitud,
            intensidad = Math.Clamp(
                (double)(puntuacionesPorMunicipio[establecimiento.IdMunicipio] / 100m),
                0d,
                1d)
        });

        return Ok(puntos);
    }

    private bool TryGetAuthenticatedUserId(out int userId)
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out userId);
    }
}
