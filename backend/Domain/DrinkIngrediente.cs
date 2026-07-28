using System.ComponentModel.DataAnnotations;

namespace BigCup.Domain
{
    public class DrinkIngrediente
    {
        public int DrinkId { get; set; }
        public Drink Drink { get; set; } = null!;

        public int IngredienteId { get; set; }
        public Ingrediente Ingrediente { get; set; } = null!;

        public decimal Quantidade { get; set; }

        [MaxLength(50)]
        public string? Unidade { get; set; }
    }
}
