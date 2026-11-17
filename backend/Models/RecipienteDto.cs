using BigCup.Domain;

namespace BigCup.Models
{
    public sealed record RecipienteDto(int Id, string Nome, int CapacidadeMl)
    {
        public static RecipienteDto FromEntity(Recipiente r) => new(r.Id, r.Nome, r.CapacidadeMl);
    }
}
