using BigCup.Domain;
using Microsoft.EntityFrameworkCore;

namespace BigCup.Data
{
    /// <summary>
    /// Dados iniciais. Executado pelo EF (UseSeeding/UseAsyncSeeding) a cada Migrate ou `dotnet ef database update`;
    /// é idempotente. Recipientes não estão aqui: são dados fixos criados pela migration (HasData).
    /// </summary>
    public static class SeedData
    {
        public const string NomeDrinkExemplo = "Margarita Clássica";

        private static readonly (string Nome, decimal Quantidade, string Unidade)[] IngredientesExemplo =
        {
            ("Tequila", 45, "ml"),
            ("Cointreau", 20, "ml"),
            ("Suco de limão", 15, "ml")
        };

        private static readonly string[] PassosExemplo =
        {
            "Adicione todos os ingredientes em uma coqueteleira com gelo.",
            "Agite vigorosamente por alguns segundos.",
            "Coe em um copo gelado e sirva imediatamente."
        };

        public static void Seed(DbContext context) => SeedAsync(context, CancellationToken.None).GetAwaiter().GetResult();

        public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
        {
            var drinks = context.Set<Drink>();
            if (await drinks.AnyAsync(d => d.UsuarioId == null && d.Nome == NomeDrinkExemplo, cancellationToken))
            {
                return;
            }

            var ingredientes = context.Set<Ingrediente>();
            var drink = new Drink
            {
                Nome = NomeDrinkExemplo,
                Descricao = "Drink de exemplo, visível para todos os usuários.",
                TempoPreparoMinutos = 5,
                RecipienteId = 2, // Copo pequeno (300 ml)
                PassosPreparo = PassosExemplo
                    .Select((instrucao, i) => new PassoPreparo { Ordem = i + 1, Instrucao = instrucao })
                    .ToList()
            };

            foreach (var (nome, quantidade, unidade) in IngredientesExemplo)
            {
                var ingrediente = await ingredientes.FirstOrDefaultAsync(i => i.Nome == nome, cancellationToken)
                                  ?? new Ingrediente { Nome = nome };
                drink.DrinkIngredientes.Add(new DrinkIngrediente
                {
                    Ingrediente = ingrediente,
                    Quantidade = quantidade,
                    Unidade = unidade
                });
            }

            drinks.Add(drink);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
