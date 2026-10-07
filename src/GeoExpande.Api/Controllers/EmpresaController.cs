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
