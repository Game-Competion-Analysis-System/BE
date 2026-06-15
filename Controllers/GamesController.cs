using GameCompetionAnalysisSystem.Models;
using GameCompetionAnalysisSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameCompetionAnalysisSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _service;

        public GamesController(IGameService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_service.GetAllGames());

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var game = _service.GetById(id);
            if (game == null) return NotFound();
            return Ok(game);
        }

        [HttpPost]
        public IActionResult Create(Game game)
        {
            _service.Add(game);
            return Ok(game);
        }

        [HttpGet("mmorpg")]
        public IActionResult GetMMORPG() => Ok(_service.GetMMORPGGames());

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Game game)
        {
            game.Gameid = id;
            _service.Update(game);
            return Ok(game);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var game = _service.GetById(id);
            if (game == null) return NotFound();
            _service.Delete(id);
            return Ok(new { message = "Game deleted successfully" });
        }
    }
}
