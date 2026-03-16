using Microsoft.EntityFrameworkCore;
using BIL.Service;
using DAL.DTO;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameCompetionAnalysisSystem.Controllers
{
    [ApiController]
    [Route("api/guilds")]
    [Authorize]
    public class GuildsController(IGuildService service) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetList([FromQuery] QueryParameters parameters) => Ok(service.GetAll(parameters));

        [HttpGet("search")]
        [AllowAnonymous]
        public IActionResult Search([FromQuery] string name) => Ok(service.SearchByName(name));

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetById(int id)
        {
            var guild = service.GetById(id);
            if (guild == null) return NotFound();
            return Ok(guild);
        }

        [HttpGet("server/{serverId}")]
        [AllowAnonymous]
        public IActionResult GetByServer(int serverId) => Ok(service.GetByServer(serverId));

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Create(Guild guild)
        {
            service.Add(guild);
            return Ok(new GuildDto
            {
                GuildId = guild.Guildid,
                GuildName = guild.Guildname
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Update(int id, [FromBody] Guild guild)
        {
            guild.Guildid = id;
            service.Update(guild);
            return Ok(new GuildDto
            {
                GuildId = guild.Guildid,
                GuildName = guild.Guildname
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            service.Delete(id);
            return Ok(new { message = "Guild deleted successfully" });
        }
    }
}
