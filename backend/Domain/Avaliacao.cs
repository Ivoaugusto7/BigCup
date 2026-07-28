using System;
using System.ComponentModel.DataAnnotations;

namespace BigCup.Domain
{
    public class Avaliacao
    {
        public int Id { get; set; }

        public int DrinkId { get; set; }
        public Drink Drink { get; set; } = null!;

        [Range(1, 5)]
        public int Nota { get; set; }

        [MaxLength(500)]
        public string? Comentario { get; set; }

        public DateTime DataAvaliacao { get; set; } = DateTime.UtcNow;
    }
}
