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
[Route("api/empresa")]
public class EmpresaController : ControllerBase
{
    private readonly GeoExpandeDbContext _context;

    public EmpresaController(GeoExpandeDbContext context)
    {
        _context = context;
    }

    [HttpGet("perfil")]
    public async Task<ActionResult<EmpresaPerfilDto>> GetPerfil()
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized();

        var usuario = await _context.Usuarios
            .AsNoTracking()
            .Include(u => u.IdEmpresaNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == userId);

        if (usuario?.IdEmpresaNavigation is not { } empresa)
            return NotFound("El usuario no tiene una empresa asociada.");

        return Ok(ToPerfilDto(empresa));
    }

    [HttpPut("actualizar")]
    public async Task<IActionResult> ActualizarPerfil(EmpresaPerfilDto dto)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized();

        var usuario = await _context.Usuarios
            .Include(u => u.IdEmpresaNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == userId);

        if (usuario?.IdEmpresaNavigation is not { } empresa)
            return NotFound("El usuario no tiene una empresa asociada.");

        var giroPerteneceAlSector = await _context.Giros
            .AnyAsync(giro => giro.IdGiro == dto.IdGiro && giro.IdSector == dto.IdSector);
        if (!giroPerteneceAlSector)
            return BadRequest("El giro seleccionado no pertenece al sector indicado.");

        empresa.Correo = dto.Correo;
        empresa.RazonSocial = dto.RazonSocial;
        empresa.NombreComercial = dto.NombreComercial;
        empresa.CiudadBase = dto.CiudadBase;
        empresa.IdSector = dto.IdSector;
        empresa.IdGiro = dto.IdGiro;
        empresa.Tamano = dto.Tamano;
        empresa.DescripcionNegocio = dto.DescripcionNegocio;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("recomendar-zonas")]
    public async Task<IActionResult> RecomendarZonas([FromQuery] RecomendarZonasRequestDto request)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized();

        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdUsuario == userId);

        if (usuario?.IdEmpresa is not int empresaId)
            return NotFound("El usuario no tiene una empresa asociada.");

        var idGiroEmpresa = await _context.Empresas
            .AsNoTracking()
            .Where(empresa => empresa.IdEmpresa == empresaId)
            .Select(empresa => empresa.IdGiro)
            .FirstOrDefaultAsync();
        if (idGiroEmpresa is not int giroEmpresa)
            return BadRequest("La empresa no tiene un giro configurado en su perfil.");
        if (request.IdGiro != giroEmpresa)
            return BadRequest("El giro solicitado debe coincidir con el giro configurado en el perfil de la empresa.");

        var giro = await _context.Giros
            .AsNoTracking()
            .FirstOrDefaultAsync(giro => giro.IdGiro == giroEmpresa);
        if (giro is null)
            return BadRequest("El giro indicado no existe.");

        var radioKm = request.RadioKm
            ?? await _context.PreferenciasExpansions
                .AsNoTracking()
                .Where(preferencia => preferencia.IdEmpresa == empresaId)
                .OrderByDescending(preferencia => preferencia.FechaActualizacion)
                .Select(preferencia => preferencia.RadioAnalisisKm)
                .FirstOrDefaultAsync()
            ?? 10m;

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var ahora = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var analisis = new Analisi
            {
                IdEmpresa = empresaId,
                IdUsuario = userId,
                Nombre = $"Recomendaciones de zonas - {giro.Nombre}",
                Descripcion = "Búsqueda de expansión. Se guardaron las cinco zonas con mayor puntuación.",
                Estado = "Completado",
                FechaInicio = ahora,
                FechaFinalizacion = ahora
            };
            _context.Analises.Add(analisis);
            await _context.SaveChangesAsync();

            var escenario = new EscenariosExpansion
            {
                IdAnalisis = analisis.IdAnalisis,
                Nombre = "Top 5 zonas recomendadas",
                Descripcion = "Las cinco zonas con mayor puntuación calculada para el giro seleccionado.",
                RadioBusquedaKm = radioKm
            };
            _context.EscenariosExpansions.Add(escenario);
            await _context.SaveChangesAsync();

            var filasInsertadas = await _context.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO ubicaciones_recomendadas (
                    id_escenario, id_estado, id_municipio, id_localidad,
                    nombre_ubicacion, ubicacion, puntuacion,
                    puntuacion_competencia, puntuacion_poblacion,
                    puntuacion_proveedores, puntuacion_accesibilidad,
                    competidores_cercanos, proveedores_cercanos,
                    cuota_presencia_estimada, demanda_estimada_mensual_mxn,
                    nivel_riesgo, recomendacion, ranking
                )
                SELECT
                    {escenario.IdEscenario}, z.id_estado, z.id_municipio, NULL,
                    z.nombre_zona, z.ubicacion_zona, z.puntuacion,
                    z.puntuacion_competencia, z.puntuacion_poblacion,
                    NULL, z.puntuacion_accesibilidad,
                    z.competidores_cercanos::integer, NULL,
                    z.cuota_presencia_estimada, z.demanda_estimada_mensual_mxn,
                    z.nivel_riesgo, z.recomendacion,
                    ROW_NUMBER() OVER (ORDER BY z.puntuacion DESC)::integer
                FROM fn_recomendar_zonas({request.IdGiro}, {empresaId}, {radioKm}) z
                ORDER BY z.puntuacion DESC
                LIMIT 5
                """);

            if (filasInsertadas == 0)
            {
                await transaction.RollbackAsync();
                return NotFound("No se encontraron zonas para generar recomendaciones.");
            }

            var recomendaciones = await _context.Database
                .SqlQuery<RecomendacionGuardadaDto>($"""
                    SELECT
                        z.id_ubicacion AS "IdUbicacion",
                        z.id_estado AS "IdEstado",
                        e.nombre AS "Estado",
                        z.id_municipio AS "IdMunicipio",
                        m.nombre AS "Municipio",
                        z.id_localidad AS "IdLocalidad",
                        z.nombre_ubicacion AS "NombreUbicacion",
                        ST_Y(z.ubicacion::geometry) AS "Lat",
                        ST_X(z.ubicacion::geometry) AS "Lng",
                        z.puntuacion AS "Puntuacion",
                        z.puntuacion_competencia AS "PuntuacionCompetencia",
                        z.puntuacion_poblacion AS "PuntuacionPoblacion",
                        z.puntuacion_proveedores AS "PuntuacionProveedores",
                        z.puntuacion_accesibilidad AS "PuntuacionAccesibilidad",
                        z.competidores_cercanos AS "CompetidoresCercanos",
                        z.proveedores_cercanos AS "ProveedoresCercanos",
                        z.cuota_presencia_estimada AS "CuotaPresenciaEstimada",
                        z.demanda_estimada_mensual_mxn AS "DemandaEstimadaMensualMxn",
                        z.nivel_riesgo AS "NivelRiesgo",
                        z.recomendacion AS "Recomendacion",
                        z.ranking AS "Ranking",
                        z.fecha_calculo AS "FechaCalculo"
                    FROM ubicaciones_recomendadas z
                    JOIN estados e ON e.id_estado = z.id_estado
                    JOIN municipios m ON m.id_municipio = z.id_municipio
                    WHERE z.id_escenario = {escenario.IdEscenario}
                    ORDER BY z.ranking
                    """)
                .ToListAsync();

            await transaction.CommitAsync();
            return Ok(new
            {
                idAnalisis = analisis.IdAnalisis,
                idEscenario = escenario.IdEscenario,
                zonasGuardadas = recomendaciones.Count,
                recomendaciones
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

    }

    private bool TryGetAuthenticatedUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdClaim, out userId);
    }

    private static EmpresaPerfilDto ToPerfilDto(Empresa empresa) => new()
    {
        Correo = empresa.Correo ?? string.Empty,
        RazonSocial = empresa.RazonSocial ?? string.Empty,
        NombreComercial = empresa.NombreComercial,
        CiudadBase = empresa.CiudadBase ?? string.Empty,
        IdSector = empresa.IdSector ?? 0,
        IdGiro = empresa.IdGiro ?? 0,
        Tamano = empresa.Tamano ?? string.Empty,
        DescripcionNegocio = empresa.DescripcionNegocio ?? string.Empty
    };
}
