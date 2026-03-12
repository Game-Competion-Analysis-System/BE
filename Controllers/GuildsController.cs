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
    public class GuildsController(IGuildService service) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll() => Ok(service.GetAll());

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
            return Ok(guild);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Update(int id, [FromBody] Guild guild)
        {
            guild.Guildid = id;
            service.Update(guild);
            return Ok(guild);
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
