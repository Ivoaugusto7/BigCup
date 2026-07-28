using BigCup.Data;
using BigCup.Domain;
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
        public async Task<ActionResult<IEnumerable<Drink>>> GetDrinks()
        {
            var drinks = await _context.Drinks
                .Include(d => d.DrinkIngredientes)
                    .ThenInclude(di => di.Ingrediente)
                .Include(d => d.PassosPreparo)
                .Include(d => d.Avaliacoes)
                .ToListAsync();

            return Ok(drinks);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Drink>> GetDrink(int id)
        {
            var drink = await _context.Drinks
                .Include(d => d.DrinkIngredientes)
                    .ThenInclude(di => di.Ingrediente)
                .Include(d => d.PassosPreparo)
                .Include(d => d.Avaliacoes)
                .FirstOrDefaultAsync(d => d.Id == id);

            return drink is null ? NotFound() : Ok(drink);
        }

        [HttpPost]
        public async Task<ActionResult<Drink>> CreateDrink(Drink drink)
        {
            _context.Drinks.Add(drink);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDrink), new { id = drink.Id }, drink);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateDrink(int id, Drink drink)
        {
            if (id != drink.Id)
            {
                return BadRequest();
            }

            if (!await _context.Drinks.AnyAsync(d => d.Id == id))
            {
                return NotFound();
            }

            _context.Entry(drink).State = EntityState.Modified;
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
    }
}
