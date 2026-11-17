using System.ComponentModel.DataAnnotations;

namespace BigCup.Domain
{
    /// <summary>
    /// Avaliação privada: só o próprio usuário vê. Uma por usuário por drink.
    /// </summary>
    public class Avaliacao
    {
        public int Id { get; set; }

        public int DrinkId { get; set; }
        public Drink Drink { get; set; } = null!;

        public string UsuarioId { get; set; } = string.Empty;
        public Usuario Usuario { get; set; } = null!;

        [Range(1, 5)]
        public int Nota { get; set; }

        [MaxLength(500)]
        public string? Comentario { get; set; }

        public DateTime DataAvaliacao { get; set; } = DateTime.UtcNow;
    }
}
