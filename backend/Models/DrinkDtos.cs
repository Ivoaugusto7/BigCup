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
        IReadOnlyList<DrinkIngredienteDto> Ingredientes,
        IReadOnlyList<PassoPreparoDto> Passos,
        IReadOnlyList<AvaliacaoDto> Avaliacoes,
        double? MediaAvaliacoes)
    {
        public static DrinkDto FromEntity(Drink drink) => new(
            drink.Id,
            drink.Nome,
            drink.Descricao,
            drink.TempoPreparoMinutos,
            drink.DrinkIngredientes
                .Select(di => new DrinkIngredienteDto(di.IngredienteId, di.Ingrediente.Nome, di.Quantidade, di.Unidade))
                .ToList(),
            drink.PassosPreparo
                .OrderBy(p => p.Ordem)
                .Select(p => new PassoPreparoDto(p.Id, p.Ordem, p.Instrucao))
                .ToList(),
            drink.Avaliacoes
                .OrderByDescending(a => a.DataAvaliacao)
                .Select(a => new AvaliacaoDto(a.Id, a.Nota, a.Comentario, a.DataAvaliacao))
                .ToList(),
            drink.Avaliacoes.Count == 0 ? null : Math.Round(drink.Avaliacoes.Average(a => a.Nota), 2));
    }

    public sealed record DrinkIngredienteDto(int IngredienteId, string Nome, decimal Quantidade, string? Unidade);

    public sealed record PassoPreparoDto(int Id, int Ordem, string Instrucao);

    public sealed record AvaliacaoDto(int Id, int Nota, string? Comentario, DateTime DataAvaliacao);
}
