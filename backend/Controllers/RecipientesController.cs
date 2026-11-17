using BigCup.Data;
using BigCup.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BigCup.Controllers
{
    /// <summary>Recipientes disponíveis (público, sem login).</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RecipientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RecipientesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecipienteDto>>> GetRecipientes()
        {
            var recipientes = await _context.Recipientes
                .AsNoTracking()
                .OrderBy(r => r.CapacidadeMl)
                .Select(r => new RecipienteDto(r.Id, r.Nome, r.CapacidadeMl))
                .ToListAsync();

            return Ok(recipientes);
        }
    }
}
