using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BigCup.Domain
{
    public class Ingrediente
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Unidade { get; set; }

        public ICollection<DrinkIngrediente> DrinkIngredientes { get; set; } = new List<DrinkIngrediente>();
    }
}
