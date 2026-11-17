using System.ComponentModel.DataAnnotations;

namespace BigCup.Domain
{
    public class Drink
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descricao { get; set; }

        public int TempoPreparoMinutos { get; set; }

        /// <summary>Dono do drink. <c>null</c> = drink público de exemplo (somente leitura).</summary>
        public string? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public int? RecipienteId { get; set; }
        public Recipiente? Recipiente { get; set; }

        public ICollection<DrinkIngrediente> DrinkIngredientes { get; set; } = new List<DrinkIngrediente>();
        public ICollection<PassoPreparo> PassosPreparo { get; set; } = new List<PassoPreparo>();
        public ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
    }
}
