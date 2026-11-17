using System.Security.Claims;

namespace BigCup.Controllers
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>Id do usuário logado (AspNetUsers.Id). Só use em endpoints com [Authorize].</summary>
        public static string GetUsuarioId(this ClaimsPrincipal user) =>
            user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Usuário não autenticado.");
    }
}
