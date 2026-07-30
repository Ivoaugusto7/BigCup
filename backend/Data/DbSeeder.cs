using BigCup.Domain;
using Microsoft.EntityFrameworkCore;

namespace BigCup.Data
{
    public class DbSeeder
    {
        private readonly AppDbContext _context;

        public DbSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            await _context.Database.MigrateAsync(cancellationToken);

            if (await _context.Drinks.AnyAsync(cancellationToken))
            {
                return;
            }

            var tequila = new Ingrediente { Nome = "Tequila", Unidade = "ml" };
            var cointreau = new Ingrediente { Nome = "Cointreau", Unidade = "ml" };
            var sucoLimao = new Ingrediente { Nome = "Suco de limão", Unidade = "ml" };

            _context.Ingredientes.AddRange(tequila, cointreau, sucoLimao);
            await _context.SaveChangesAsync(cancellationToken);

            var drink = new Drink
            {
                Nome = "Margarita Clássica",
                Descricao = "Uma receita premium para o Drink Builder com proporções simples e visual elegante.",
                TempoPreparoMinutos = 5,
                DrinkIngredientes = new List<DrinkIngrediente>
                {
                    new() { Ingrediente = tequila, Quantidade = 45, Unidade = "ml" },
                    new() { Ingrediente = cointreau, Quantidade = 20, Unidade = "ml" },
                    new() { Ingrediente = sucoLimao, Quantidade = 15, Unidade = "ml" }
                },
                PassosPreparo = new List<PassoPreparo>
                {
                    new() { Ordem = 1, Instrucao = "Adicione todos os ingredientes em uma coqueteleira com gelo." },
                    new() { Ordem = 2, Instrucao = "Agite vigorosamente por alguns segundos." },
                    new() { Ordem = 3, Instrucao = "Coe em um copo gelado e sirva imediatamente." }
                }
            };

            _context.Drinks.Add(drink);
            await _context.SaveChangesAsync(cancellationToken);

            _context.Avaliacoes.Add(new Avaliacao
            {
                Drink = drink,
                Nota = 5,
                Comentario = "Receita equilibrada e fácil de preparar.",
                DataAvaliacao = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
