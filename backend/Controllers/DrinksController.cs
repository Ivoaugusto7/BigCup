using BigCup.Data;
using BigCup.Domain;
using BigCup.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BigCup.Controllers
{
    [ApiController]
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
            var drinks = await QueryDrinksCompletos()
                .AsNoTracking()
                .OrderBy(d => d.Nome)
                .ToListAsync();

            return Ok(drinks.Select(DrinkDto.FromEntity));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<DrinkDto>> GetDrink(int id)
        {
            var drink = await QueryDrinksCompletos()
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            return drink is null ? NotFound() : Ok(DrinkDto.FromEntity(drink));
        }

        [HttpPost]
        public async Task<ActionResult<DrinkDto>> CreateDrink(DrinkInputDto input)
        {
            var erro = ValidarIngredientesDuplicados(input);
            if (erro is not null)
            {
                return erro;
            }

            var ingredientes = await ResolverIngredientesAsync(input.Ingredientes);

            var drink = new Drink
            {
                Nome = input.Nome.Trim(),
                Descricao = input.Descricao,
                TempoPreparoMinutos = input.TempoPreparoMinutos,
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

            return CreatedAtAction(nameof(GetDrink), new { id = drink.Id }, DrinkDto.FromEntity(drink));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateDrink(int id, DrinkInputDto input)
        {
            var erro = ValidarIngredientesDuplicados(input);
            if (erro is not null)
            {
                return erro;
            }

            var drink = await _context.Drinks
                .Include(d => d.DrinkIngredientes)
                .Include(d => d.PassosPreparo)
                .AsSplitQuery()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (drink is null)
            {
                return NotFound();
            }

            drink.Nome = input.Nome.Trim();
            drink.Descricao = input.Descricao;
            drink.TempoPreparoMinutos = input.TempoPreparoMinutos;

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
            var drink = await _context.Drinks.FindAsync(id);
            if (drink is null)
            {
                return NotFound();
            }

            _context.Drinks.Remove(drink);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private IQueryable<Drink> QueryDrinksCompletos() => _context.Drinks
            .Include(d => d.DrinkIngredientes)
                .ThenInclude(di => di.Ingrediente)
            .Include(d => d.PassosPreparo)
            .Include(d => d.Avaliacoes)
            .AsSplitQuery();

        private ActionResult? ValidarIngredientesDuplicados(DrinkInputDto input)
        {
            var duplicados = input.Ingredientes
                .GroupBy(i => i.Nome.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicados.Count == 0)
            {
                return null;
            }

            ModelState.AddModelError(nameof(DrinkInputDto.Ingredientes),
                $"Ingrediente repetido no mesmo drink: {string.Join(", ", duplicados)}.");
            return ValidationProblem(ModelState);
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

            foreach (var item in itens)
            {
                var nome = item.Nome.Trim();
                if (!resultado.ContainsKey(nome))
                {
                    var novo = new Ingrediente { Nome = nome, Unidade = item.Unidade };
                    _context.Ingredientes.Add(novo);
                    resultado[nome] = novo;
                }
            }

            return resultado;
        }
    }
}
