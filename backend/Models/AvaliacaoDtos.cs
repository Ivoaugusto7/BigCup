using System.ComponentModel.DataAnnotations;
using BigCup.Domain;

namespace BigCup.Models
{
    public sealed class AvaliacaoInputDto
    {
        [Range(1, 5)]
        public int Nota { get; set; }

        [MaxLength(500)]
        public string? Comentario { get; set; }
    }

    public sealed record AvaliacaoDto(int Id, int DrinkId, int Nota, string? Comentario, DateTime DataAvaliacao)
    {
        public static AvaliacaoDto FromEntity(Avaliacao a) => new(a.Id, a.DrinkId, a.Nota, a.Comentario, a.DataAvaliacao);
    }

    /// <summary>Item da lista "minhas avaliações".</summary>
    public sealed record MinhaAvaliacaoDto(int Id, int DrinkId, string DrinkNome, int Nota, string? Comentario, DateTime DataAvaliacao);
}
