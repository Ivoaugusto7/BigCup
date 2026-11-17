using Microsoft.AspNetCore.Identity;

namespace BigCup.Domain
{
    /// <summary>
    /// Usuário da aplicação (ASP.NET Core Identity). Cada usuário vê apenas os próprios drinks e avaliações.
    /// </summary>
    public class Usuario : IdentityUser
    {
        public ICollection<Drink> Drinks { get; set; } = new List<Drink>();
        public ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
    }
}
