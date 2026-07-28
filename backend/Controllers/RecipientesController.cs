using BigCup.Models;
using Microsoft.AspNetCore.Mvc;

namespace BigCup.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipientesController : ControllerBase
    {
        private static readonly IReadOnlyList<RecipienteDto> Recipientes = new[]
        {
            new RecipienteDto(1, "Shot", 50),
            new RecipienteDto(2, "Copo", 300),
            new RecipienteDto(3, "Copo", 500),
            new RecipienteDto(4, "Copo", 700),
            new RecipienteDto(5, "Garrafa", 2000),
            new RecipienteDto(6, "Galão", 5000)
        };

        [HttpGet]
        public ActionResult<IEnumerable<RecipienteDto>> GetRecipientes() => Ok(Recipientes);
    }
}