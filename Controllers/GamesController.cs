using BIL.Service;
using DAL.DTO;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameCompetionAnalysisSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GamesController(IGameService service) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetList([FromQuery] QueryParameters parameters) => Ok(service.GetAllGames(parameters));

        [HttpGet("search")]
        [AllowAnonymous]
        public IActionResult Search([FromQuery] string name) => Ok(service.SearchByName(name));

        //Get by ID
        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetById(int id)
        {
            var game = service.GetById(id);
            if (game == null) return NotFound();
            return Ok(game);
        }
        //Create Game
        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Create(Game game)
        {
            service.Add(game);
            return Ok(new GameDto
            {
                GameId = game.Gameid,
                GameName = game.Gamename,
                Genre = game.Genre
            });
        }


        [HttpGet("mmorpg")]
        [AllowAnonymous]
        public IActionResult GetMMORPG()
            => Ok(service.GetMMORPGGames());

        //Update
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Update(int id, [FromBody] Game game)
        {
            game.Gameid = id;
            service.Update(game);
            return Ok(new GameDto
            {
                GameId = game.Gameid,
                GameName = game.Gamename,
                Genre = game.Genre
            });
        }
        //Delete
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            var game = service.GetById(id);
            if (game == null) return NotFound();

            service.Delete(id);
            return Ok(new { message = "Game deleted successfully" });
        }

    }

}
