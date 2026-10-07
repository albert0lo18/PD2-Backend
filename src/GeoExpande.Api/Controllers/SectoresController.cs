using GeoExpande.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoExpande.Api.Controllers;

[ApiController]
[Route("api/sectores")]
public class SectoresController : ControllerBase
{
    private readonly GeoExpandeDbContext _context;

    public SectoresController(GeoExpandeDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSectores()
    {
        var sectores = await _context.Sectores
            .AsNoTracking()
            .OrderBy(sector => sector.Nombre)
            .Select(sector => new
            {
                sector.IdSector,
                sector.Codigo,
                sector.Nombre,
                sector.Descripcion,
                sector.Activo
            })
            .ToListAsync();

        return Ok(sectores);
    }

    [HttpGet("{idSector:int}/giros")]
    public async Task<IActionResult> GetGirosPorSector(int idSector)
    {
        var giros = await _context.Giros
            .AsNoTracking()
            .Where(giro => giro.IdSector == idSector)
            .OrderBy(giro => giro.Nombre)
            .Select(giro => new
            {
                giro.IdGiro,
                giro.IdSector,
                giro.Codigo,
                giro.Nombre,
                giro.Descripcion,
                giro.Activo
            })
            .ToListAsync();

        return Ok(giros);
    }
}
