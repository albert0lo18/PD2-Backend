using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GeoExpande.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoExpande.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/simulador")]
public class SimuladorController : ControllerBase
{
    private readonly GeoExpandeDbContext _context;

    public SimuladorController(GeoExpandeDbContext context) => _context = context;

    [HttpGet("resultado")]
    public async Task<IActionResult> GetResultado([FromQuery] int idUbicacion)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized();
        if (idUbicacion <= 0)
            return BadRequest("idUbicacion debe ser un entero positivo.");

        var resultado = await _context.UbicacionesRecomendadas
            .AsNoTracking()
            .Where(z => z.IdUbicacion == idUbicacion
                && z.IdEscenarioNavigation.IdAnalisisNavigation.IdUsuario == userId)
            .Select(z => new
            {
                idUbicacion = z.IdUbicacion,
                idEstado = z.IdEstado,
                estado = z.IdEstadoNavigation.Nombre,
                idMunicipio = z.IdMunicipio,
                municipio = z.IdMunicipioNavigation.Nombre,
                competidoresCercanos = z.CompetidoresCercanos,
                cuotaPresenciaEstimada = z.CuotaPresenciaEstimada,
                demandaEstimadaMensualMxn = z.DemandaEstimadaMensualMxn,
                nivelRiesgo = z.NivelRiesgo,
                recomendacion = z.Recomendacion
            })
            .FirstOrDefaultAsync();

        return resultado is null
            ? NotFound("No se encontró el resultado solicitado.")
            : Ok(resultado);
    }

    private bool TryGetAuthenticatedUserId(out int userId)
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out userId);
    }
}
