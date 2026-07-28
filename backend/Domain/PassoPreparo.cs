using System.ComponentModel.DataAnnotations;

namespace BigCup.Domain
{
    public class PassoPreparo
    {
        public int Id { get; set; }

        public int DrinkId { get; set; }
        public Drink Drink { get; set; } = null!;

        public int Ordem { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Instrucao { get; set; } = string.Empty;
    }
}
