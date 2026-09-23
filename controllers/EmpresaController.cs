using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PD2_Backend.Models;

namespace PD2_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpresaController : ControllerBase
    {
        private readonly PlataformaB2bContext _context;

        // Aquí se inyecta automáticamente la conexión a SQL Server
        public EmpresaController(PlataformaB2bContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmpresas()
        {
            // Esto equivale a un "SELECT * FROM Empresas" en SQL Server
            var empresas = await _context.Empresas.ToListAsync();
            return Ok(empresas);
        }
    }
}