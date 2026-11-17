using BigCup.Data;
using BigCup.Domain;
using BigCup.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BigCup.Controllers
{
    /// <summary>
    /// Avaliações privadas: cada usuário vê e altera somente as próprias. Uma avaliação por drink.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api")]
    public class AvaliacoesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AvaliacoesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Todas as minhas avaliações, da mais recente para a mais antiga.</summary>
        [HttpGet("avaliacoes")]
        public async Task<ActionResult<IEnumerable<MinhaAvaliacaoDto>>> GetMinhasAvaliacoes()
        {
            var usuarioId = User.GetUsuarioId();
            var avaliacoes = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.UsuarioId == usuarioId)
                .OrderByDescending(a => a.DataAvaliacao)
                .Select(a => new MinhaAvaliacaoDto(a.Id, a.DrinkId, a.Drink.Nome, a.Nota, a.Comentario, a.DataAvaliacao))
                .ToListAsync();

            return Ok(avaliacoes);
        }

        [HttpGet("drinks/{drinkId:int}/avaliacao")]
        public async Task<ActionResult<AvaliacaoDto>> GetMinhaAvaliacao(int drinkId)
        {
            var avaliacao = await BuscarMinhaAvaliacao(drinkId).AsNoTracking().FirstOrDefaultAsync();
            return avaliacao is null ? NotFound() : Ok(AvaliacaoDto.FromEntity(avaliacao));
        }

        /// <summary>Cria ou atualiza a minha avaliação do drink.</summary>
        [HttpPut("drinks/{drinkId:int}/avaliacao")]
        public async Task<ActionResult<AvaliacaoDto>> SalvarAvaliacao(int drinkId, AvaliacaoInputDto input)
        {
            var usuarioId = User.GetUsuarioId();

            var drinkVisivel = await _context.Drinks
                .AnyAsync(d => d.Id == drinkId && (d.UsuarioId == null || d.UsuarioId == usuarioId));
            if (!drinkVisivel)
            {
                return NotFound();
            }

            var avaliacao = await BuscarMinhaAvaliacao(drinkId).FirstOrDefaultAsync();
            if (avaliacao is null)
            {
                avaliacao = new Avaliacao { DrinkId = drinkId, UsuarioId = usuarioId };
                _context.Avaliacoes.Add(avaliacao);
            }

            avaliacao.Nota = input.Nota;
            avaliacao.Comentario = input.Comentario;
            avaliacao.DataAvaliacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(AvaliacaoDto.FromEntity(avaliacao));
        }

        [HttpDelete("drinks/{drinkId:int}/avaliacao")]
        public async Task<IActionResult> RemoverAvaliacao(int drinkId)
        {
            var avaliacao = await BuscarMinhaAvaliacao(drinkId).FirstOrDefaultAsync();
            if (avaliacao is null)
            {
                return NotFound();
            }

            _context.Avaliacoes.Remove(avaliacao);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private IQueryable<Avaliacao> BuscarMinhaAvaliacao(int drinkId)
        {
            var usuarioId = User.GetUsuarioId();
            return _context.Avaliacoes.Where(a => a.DrinkId == drinkId && a.UsuarioId == usuarioId);
        }
    }
}
