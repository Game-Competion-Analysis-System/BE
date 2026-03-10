using BIL.Service;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GameCompetionAnalysisSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PlayersController(IPlayerService service) : ControllerBase
    {
        private readonly IPlayerService _service = service;

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll() => Ok(_service.GetAll());

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetById(int id)
        {
            var player = _service.GetById(id);
            if (player == null) return NotFound();
            return Ok(player);
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public IActionResult Search([FromQuery] string name) => Ok(_service.SearchByName(name));

        [HttpGet("game/{gameId}")]
        [AllowAnonymous]
        public IActionResult GetByGame(int gameId) => Ok(_service.GetByGame(gameId));

        [HttpGet("server/{serverId}")]
        [AllowAnonymous]
        public IActionResult GetByServer(int serverId) => Ok(_service.GetByServer(serverId));

        [HttpGet("guild/{guildId}")]
        [AllowAnonymous]
        public IActionResult GetByGuild(int guildId) => Ok(_service.GetByGuild(guildId));

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Create(Player player)
        {
            _service.Add(player);
            return Ok(player);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Update(int id, [FromBody] Player player)
        {
            player.Playerid = id;
            _service.Update(player);
            return Ok(player);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            _service.Delete(id);
            return Ok(new { message = "Player deleted successfully" });
        }
    }
}
