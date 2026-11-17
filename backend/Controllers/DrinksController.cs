using BigCup.Data;
using BigCup.Domain;
using BigCup.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BigCup.Controllers
{
    /// <summary>
    /// Drinks do usuário logado + drinks públicos de exemplo (somente leitura).
    /// Drinks de outros usuários nunca aparecem (404).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class DrinksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DrinksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DrinkDto>>> GetDrinks()
        {
            var usuarioId = User.GetUsuarioId();
            var drinks = await QueryDrinksVisiveis(usuarioId)
                .AsNoTracking()
                .OrderBy(d => d.UsuarioId == null) // os do usuário primeiro, exemplos depois
                .ThenBy(d => d.Nome)
                .ToListAsync();

            return Ok(drinks.Select(d => DrinkDto.FromEntity(d, usuarioId)));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<DrinkDto>> GetDrink(int id)
        {
            var usuarioId = User.GetUsuarioId();
            var drink = await QueryDrinksVisiveis(usuarioId)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            return drink is null ? NotFound() : Ok(DrinkDto.FromEntity(drink, usuarioId));
        }

        [HttpPost]
        public async Task<ActionResult<DrinkDto>> CreateDrink(DrinkInputDto input)
        {
            var erro = await ValidarEntradaAsync(input);
            if (erro is not null)
            {
                return erro;
            }

            var usuarioId = User.GetUsuarioId();
            var ingredientes = await ResolverIngredientesAsync(input.Ingredientes);

            var drink = new Drink
            {
                UsuarioId = usuarioId,
                Nome = input.Nome.Trim(),
                Descricao = input.Descricao,
                TempoPreparoMinutos = input.TempoPreparoMinutos,
                RecipienteId = input.RecipienteId,
                DrinkIngredientes = input.Ingredientes
                    .Select(i => new DrinkIngrediente
                    {
                        Ingrediente = ingredientes[i.Nome.Trim()],
                        Quantidade = i.Quantidade,
                        Unidade = i.Unidade
                    })
                    .ToList(),
                PassosPreparo = input.Passos
                    .Select(p => new PassoPreparo { Ordem = p.Ordem, Instrucao = p.Instrucao })
                    .ToList()
            };

            _context.Drinks.Add(drink);
            await _context.SaveChangesAsync();

            var criado = await QueryDrinksVisiveis(usuarioId).AsNoTracking().FirstAsync(d => d.Id == drink.Id);
            return CreatedAtAction(nameof(GetDrink), new { id = drink.Id }, DrinkDto.FromEntity(criado, usuarioId));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateDrink(int id, DrinkInputDto input)
        {
            var erro = await ValidarEntradaAsync(input);
            if (erro is not null)
            {
                return erro;
            }

            var usuarioId = User.GetUsuarioId();
            var drink = await _context.Drinks
                .Include(d => d.DrinkIngredientes)
                .Include(d => d.PassosPreparo)
                .AsSplitQuery()
                .FirstOrDefaultAsync(d => d.Id == id && d.UsuarioId == usuarioId); // só o dono edita

            if (drink is null)
            {
                return NotFound();
            }

            drink.Nome = input.Nome.Trim();
            drink.Descricao = input.Descricao;
            drink.TempoPreparoMinutos = input.TempoPreparoMinutos;
            drink.RecipienteId = input.RecipienteId;

            // Ingredientes: sincroniza pela chave (DrinkId, IngredienteId)
            var ingredientes = await ResolverIngredientesAsync(input.Ingredientes);
            var desejados = input.Ingredientes
                .Select(i => (Ingrediente: ingredientes[i.Nome.Trim()], Dados: i))
                .ToList();

            foreach (var existente in drink.DrinkIngredientes.ToList())
            {
                var novo = desejados.FirstOrDefault(d => d.Ingrediente.Id != 0 && d.Ingrediente.Id == existente.IngredienteId);
                if (novo.Dados is null)
                {
                    drink.DrinkIngredientes.Remove(existente);
                }
                else
                {
                    existente.Quantidade = novo.Dados.Quantidade;
                    existente.Unidade = novo.Dados.Unidade;
                }
            }

            foreach (var (ingrediente, dados) in desejados)
            {
                if (ingrediente.Id == 0 || drink.DrinkIngredientes.All(di => di.IngredienteId != ingrediente.Id))
                {
                    drink.DrinkIngredientes.Add(new DrinkIngrediente
                    {
                        Ingrediente = ingrediente,
                        Quantidade = dados.Quantidade,
                        Unidade = dados.Unidade
                    });
                }
            }

            // Passos: substitui a lista inteira
            _context.PassosPreparo.RemoveRange(drink.PassosPreparo);
            drink.PassosPreparo = input.Passos
                .Select(p => new PassoPreparo { Ordem = p.Ordem, Instrucao = p.Instrucao })
                .ToList();

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDrink(int id)
        {
            var usuarioId = User.GetUsuarioId();
            var drink = await _context.Drinks.FirstOrDefaultAsync(d => d.Id == id && d.UsuarioId == usuarioId);
            if (drink is null)
            {
                return NotFound();
            }

            _context.Drinks.Remove(drink);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>Drinks públicos + do usuário, com ingredientes, passos, recipiente e SOMENTE a avaliação dele.</summary>
        private IQueryable<Drink> QueryDrinksVisiveis(string usuarioId) => _context.Drinks
            .Where(d => d.UsuarioId == null || d.UsuarioId == usuarioId)
            .Include(d => d.DrinkIngredientes)
                .ThenInclude(di => di.Ingrediente)
            .Include(d => d.PassosPreparo)
            .Include(d => d.Recipiente)
            .Include(d => d.Avaliacoes.Where(a => a.UsuarioId == usuarioId))
            .AsSplitQuery();

        private async Task<ActionResult?> ValidarEntradaAsync(DrinkInputDto input)
        {
            var ingredientesRepetidos = input.Ingredientes
                .GroupBy(i => i.Nome.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (ingredientesRepetidos.Count > 0)
            {
                ModelState.AddModelError(nameof(DrinkInputDto.Ingredientes),
                    $"Ingrediente repetido no mesmo drink: {string.Join(", ", ingredientesRepetidos)}.");
            }

            var ordensRepetidas = input.Passos
                .GroupBy(p => p.Ordem)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (ordensRepetidas.Count > 0)
            {
                ModelState.AddModelError(nameof(DrinkInputDto.Passos),
                    $"Ordem de passo repetida: {string.Join(", ", ordensRepetidas)}.");
            }

            if (input.RecipienteId is int recipienteId &&
                !await _context.Recipientes.AnyAsync(r => r.Id == recipienteId))
            {
                ModelState.AddModelError(nameof(DrinkInputDto.RecipienteId), $"Recipiente {recipienteId} não existe.");
            }

            return ModelState.IsValid ? null : ValidationProblem(ModelState);
        }

        /// <summary>
        /// Busca os ingredientes pelo nome (sem diferenciar maiúsculas) e cria os que ainda não existem.
        /// </summary>
        private async Task<Dictionary<string, Ingrediente>> ResolverIngredientesAsync(
            IEnumerable<DrinkIngredienteInputDto> itens)
        {
            var nomes = itens.Select(i => i.Nome.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var resultado = new Dictionary<string, Ingrediente>(StringComparer.OrdinalIgnoreCase);

            if (nomes.Count == 0)
            {
                return resultado;
            }

            var existentes = await _context.Ingredientes
                .Where(i => nomes.Contains(i.Nome))
                .ToListAsync();

            foreach (var ingrediente in existentes)
            {
                resultado.TryAdd(ingrediente.Nome.Trim(), ingrediente);
            }

            foreach (var nome in nomes)
            {
                if (!resultado.ContainsKey(nome))
                {
                    var novo = new Ingrediente { Nome = nome };
                    _context.Ingredientes.Add(novo);
                    resultado[nome] = novo;
                }
            }

            return resultado;
        }
    }
}
