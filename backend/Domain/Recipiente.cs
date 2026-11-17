namespace BigCup.Domain
{
    /// <summary>
    /// Recipiente em que o drink é servido (dados fixos, criados pela migration).
    /// </summary>
    public class Recipiente
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public int CapacidadeMl { get; set; }

        public ICollection<Drink> Drinks { get; set; } = new List<Drink>();
    }
}
