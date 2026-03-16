using BIL.Service;
using DAL.DTO;
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
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetList([FromQuery] QueryParameters parameters) => Ok(service.GetAll(parameters));

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetById(int id)
        {
            var player = service.GetById(id);
            if (player == null) return NotFound();
            return Ok(player);
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public IActionResult Search([FromQuery] string name) => Ok(service.SearchByName(name));

        [HttpGet("game/{gameId}")]
        [AllowAnonymous]
        public IActionResult GetByGame(int gameId) => Ok(service.GetByGame(gameId));

        [HttpGet("server/{serverId}")]
        [AllowAnonymous]
        public IActionResult GetByServer(int serverId) => Ok(service.GetByServer(serverId));

        [HttpGet("guild/{guildId}")]
        [AllowAnonymous]
        public IActionResult GetByGuild(int guildId) => Ok(service.GetByGuild(guildId));

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Create(Player player)
        {
            service.Add(player);
            return Ok(new PlayerDto
            {
                PlayerId = player.Playerid,
                PlayerName = player.Playername,
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Update(int id, [FromBody] Player player)
        {
            player.Playerid = id;
            service.Update(player);
            return Ok(new PlayerDto
            {
                PlayerId = player.Playerid,
                PlayerName = player.Playername,
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            service.Delete(id);
            return Ok(new { message = "Player deleted successfully" });
        }
    }
}
