using System.ComponentModel.DataAnnotations;
using BigCup.Domain;

namespace BigCup.Models
{
    // ---------- Entrada (POST / PUT) ----------

    public sealed class DrinkInputDto
    {
        [Required]
        [MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descricao { get; set; }

        [Range(0, int.MaxValue)]
        public int TempoPreparoMinutos { get; set; }

        /// <summary>Recipiente escolhido (ver GET /api/recipientes). Opcional.</summary>
        public int? RecipienteId { get; set; }

        public List<DrinkIngredienteInputDto> Ingredientes { get; set; } = new();

        public List<PassoPreparoInputDto> Passos { get; set; } = new();
    }

    public sealed class DrinkIngredienteInputDto
    {
        [Required]
        [MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.001", "999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
        public decimal Quantidade { get; set; }

        [MaxLength(50)]
        public string? Unidade { get; set; }
    }

    public sealed class PassoPreparoInputDto
    {
        [Range(1, int.MaxValue)]
        public int Ordem { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Instrucao { get; set; } = string.Empty;
    }

    // ---------- Saída ----------

    public sealed record DrinkDto(
        int Id,
        string Nome,
        string? Descricao,
        int TempoPreparoMinutos,
        RecipienteDto? Recipiente,
        bool Proprio,
        IReadOnlyList<DrinkIngredienteDto> Ingredientes,
        IReadOnlyList<PassoPreparoDto> Passos,
        AvaliacaoDto? MinhaAvaliacao)
    {
        /// <param name="drink">Drink carregado com ingredientes, passos, recipiente e avaliações.</param>
        /// <param name="usuarioId">Usuário logado: define <c>Proprio</c> e qual avaliação aparece.</param>
        public static DrinkDto FromEntity(Drink drink, string usuarioId)
        {
            var minha = drink.Avaliacoes.FirstOrDefault(a => a.UsuarioId == usuarioId);

            return new DrinkDto(
                drink.Id,
                drink.Nome,
                drink.Descricao,
                drink.TempoPreparoMinutos,
                drink.Recipiente is null ? null : RecipienteDto.FromEntity(drink.Recipiente),
                drink.UsuarioId == usuarioId,
                drink.DrinkIngredientes
                    .OrderBy(di => di.Ingrediente.Nome)
                    .Select(di => new DrinkIngredienteDto(di.IngredienteId, di.Ingrediente.Nome, di.Quantidade, di.Unidade))
                    .ToList(),
                drink.PassosPreparo
                    .OrderBy(p => p.Ordem)
                    .Select(p => new PassoPreparoDto(p.Id, p.Ordem, p.Instrucao))
                    .ToList(),
                minha is null ? null : AvaliacaoDto.FromEntity(minha));
        }
    }

    public sealed record DrinkIngredienteDto(int IngredienteId, string Nome, decimal Quantidade, string? Unidade);

    public sealed record PassoPreparoDto(int Id, int Ordem, string Instrucao);
}
