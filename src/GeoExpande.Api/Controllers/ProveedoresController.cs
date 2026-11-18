using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GeoExpande.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoExpande.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/proveedores")]
public class ProveedoresController : ControllerBase
{
    private readonly GeoExpandeDbContext _context;

    public ProveedoresController(GeoExpandeDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetEstablecimientos(
        [FromQuery, Range(1, int.MaxValue)] int? analisisId = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 12,
        [FromQuery, Range(1, int.MaxValue)] int? sectorId = null,
        [FromQuery, Range(1, int.MaxValue)] int? giroId = null,
        [FromQuery, StringLength(120)] string? busqueda = null)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized();

        var query = _context.EstablecimientosDenues.AsNoTracking()
            .AsQueryable();

        if (analisisId.HasValue)
        {
            var analisisExiste = await _context.Analises.AsNoTracking()
                .AnyAsync(item => item.IdAnalisis == analisisId.Value && item.IdUsuario == userId);
            if (!analisisExiste)
                return NotFound("No se encontró el análisis solicitado.");

            var municipios = await _context.UbicacionesRecomendadas.AsNoTracking()
                .Where(zona => zona.IdEscenarioNavigation.IdAnalisis == analisisId.Value)
                .Select(zona => zona.IdMunicipio)
                .Distinct()
                .ToListAsync();

            if (municipios.Count == 0)
                return Ok(new { page, pageSize, totalItems = 0, totalPages = 0, items = Array.Empty<object>() });

            query = query.Where(item => municipios.Contains(item.IdMunicipio));
        }

        if (sectorId.HasValue)
            query = query.Where(item => item.IdSector == sectorId.Value);
        if (giroId.HasValue)
            query = query.Where(item => item.IdGiro == giroId.Value);

        var termino = busqueda?.Trim();
        if (!string.IsNullOrWhiteSpace(termino))
        {
            var patron = $"%{termino}%";
            query = query.Where(item =>
                (item.Nombre != null && EF.Functions.ILike(item.Nombre, patron))
                || (item.RazonSocial != null && EF.Functions.ILike(item.RazonSocial, patron))
                || (item.ActividadEconomica != null && EF.Functions.ILike(item.ActividadEconomica, patron))
                || (item.Sector != null && EF.Functions.ILike(item.Sector, patron))
                || EF.Functions.ILike(item.IdMunicipioNavigation.Nombre, patron));
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        var skip = (int)Math.Min((long)(page - 1) * pageSize, int.MaxValue);

        var items = await query
            .OrderBy(item => item.Nombre)
            .ThenBy(item => item.IdEstablecimiento)
            .Skip(skip)
            .Take(pageSize)
            .Select(item => new
            {
                idEstablecimiento = item.IdEstablecimiento,
                nombre = item.Nombre,
                razonSocial = item.RazonSocial,
                actividadEconomica = item.ActividadEconomica,
                codigoActividad = item.CodigoActividad,
                sector = item.Sector ?? item.IdSectorNavigation!.Nombre,
                idSector = item.IdSector,
                giro = item.IdGiroNavigation == null ? null : item.IdGiroNavigation.Nombre,
                municipio = item.IdMunicipioNavigation.Nombre,
                estado = item.IdEstadoNavigation.Nombre,
                calle = item.Calle,
                numeroExterior = item.NumeroExterior,
                numeroInterior = item.NumeroInterior,
                colonia = item.Colonia,
                codigoPostal = item.CodigoPostal,
                latitud = item.Latitud,
                longitud = item.Longitud,
                fechaActualizacion = item.FechaActualizacion
            })
            .ToListAsync();

        return Ok(new { page, pageSize, totalItems, totalPages, items });
    }

    private bool TryGetAuthenticatedUserId(out int userId)
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out userId);
    }
}
