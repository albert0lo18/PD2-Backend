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
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly GeoExpandeDbContext _context;

    public DashboardController(GeoExpandeDbContext context) => _context = context;

    [HttpGet("resumen")]
    public async Task<ActionResult<DashboardResumenDto>> GetResumen()
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized();

        var empresaIdUsuario = await _context.Usuarios
            .AsNoTracking()
            .Where(item => item.IdUsuario == userId)
            .Select(item => item.IdEmpresa)
            .FirstOrDefaultAsync();

        if (empresaIdUsuario is not int empresaId)
            return NotFound("El usuario no tiene una empresa asociada.");

        var idGiro = await _context.Empresas
            .AsNoTracking()
            .Where(empresa => empresa.IdEmpresa == empresaId)
            .Select(empresa => empresa.IdGiro)
            .FirstOrDefaultAsync();

        var analisisQuery = _context.Analises
            .AsNoTracking()
            .Where(analisis => analisis.IdUsuario == userId);

        var analisisRealizados = await analisisQuery.CountAsync();

        var zonasQuery = _context.UbicacionesRecomendadas
            .AsNoTracking()
            .Where(zona => zona.IdEscenarioNavigation.IdAnalisisNavigation.IdUsuario == userId);

        var zonasEvaluadas = await zonasQuery.CountAsync();
        var zonasAltoPotencial = await zonasQuery.CountAsync(zona => zona.Puntuacion >= 80);

        IQueryable<EstablecimientosDenue> establecimientosQuery = _context.EstablecimientosDenues.AsNoTracking();
        if (idGiro.HasValue)
            establecimientosQuery = establecimientosQuery.Where(est => est.IdGiro == idGiro);
        else
            establecimientosQuery = establecimientosQuery.Where(_ => false);

        var establecimientosGiro = await establecimientosQuery.CountAsync();
        var ultimaActualizacionDenue = await establecimientosQuery
            .Where(est => est.FechaActualizacion.HasValue)
            .MaxAsync(est => (DateOnly?)est.FechaActualizacion);

        var topCiudades = await zonasQuery
            .GroupBy(zona => new
            {
                zona.IdMunicipio,
                Ciudad = zona.IdMunicipioNavigation.Nombre
            })
            .Select(grupo => new
            {
                grupo.Key.IdMunicipio,
                grupo.Key.Ciudad,
                Puntuacion = grupo.Max(zona => zona.Puntuacion)
            })
            .OrderByDescending(ciudad => ciudad.Puntuacion)
            .ThenBy(ciudad => ciudad.Ciudad)
            .Take(5)
            .ToListAsync();

        var municipioIds = topCiudades.Select(ciudad => ciudad.IdMunicipio).ToList();
        var establecimientosPorMunicipio = idGiro.HasValue && municipioIds.Count > 0
            ? await _context.EstablecimientosDenues
                .AsNoTracking()
                .Where(est => est.IdGiro == idGiro && municipioIds.Contains(est.IdMunicipio))
                .GroupBy(est => est.IdMunicipio)
                .Select(grupo => new { IdMunicipio = grupo.Key, Total = grupo.Count() })
                .ToDictionaryAsync(item => item.IdMunicipio, item => item.Total)
            : new Dictionary<int, int>();

        var resumen = new DashboardResumenDto
        {
            AnalisisRealizados = analisisRealizados,
            ZonasEvaluadas = zonasEvaluadas,
            ZonasAltoPotencial = zonasAltoPotencial,
            EstablecimientosGiro = establecimientosGiro,
            UltimaActualizacionDenue = ultimaActualizacionDenue,
            Metricas =
            [
                new() { Etiqueta = "Análisis realizados", Valor = analisisRealizados, Delta = "En tu cuenta" },
                new() { Etiqueta = "Zonas evaluadas", Valor = zonasEvaluadas, Delta = "Recomendaciones guardadas" },
                new() { Etiqueta = "Zonas con alto potencial", Valor = zonasAltoPotencial, Delta = "Puntuación de 80 o más" },
                new() { Etiqueta = "Establecimientos del giro", Valor = establecimientosGiro, Delta = "Registros DENUE" }
            ],
            ComparativaCiudades = topCiudades.Select(ciudad => new ComparativaCiudadDto
            {
                Ciudad = ciudad.Ciudad,
                Puntuacion = Math.Clamp(ciudad.Puntuacion, 0m, 100m),
                EstablecimientosGiro = establecimientosPorMunicipio.GetValueOrDefault(ciudad.IdMunicipio)
            }).ToList()
        };

        return Ok(resumen);
    }

    private bool TryGetAuthenticatedUserId(out int userId)
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out userId);
    }
}
