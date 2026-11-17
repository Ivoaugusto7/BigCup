using System.ComponentModel.DataAnnotations;

namespace BigCup.Domain
{
    /// <summary>
    /// Catálogo global de ingredientes (nome único, sem diferenciar maiúsculas).
    /// A unidade de medida fica em <see cref="DrinkIngrediente"/>, pois depende da receita.
    /// </summary>
    public class Ingrediente
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        public ICollection<DrinkIngrediente> DrinkIngredientes { get; set; } = new List<DrinkIngrediente>();
    }
}
