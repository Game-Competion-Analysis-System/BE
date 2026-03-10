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
        private readonly IGuildService _service = service;

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll() => Ok(_service.GetAll());

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetById(int id)
        {
            var guild = _service.GetById(id);
            if (guild == null) return NotFound();
            return Ok(guild);
        }

        [HttpGet("server/{serverId}")]
        [AllowAnonymous]
        public IActionResult GetByServer(int serverId) => Ok(_service.GetByServer(serverId));

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Create(Guild guild)
        {
            _service.Add(guild);
            return Ok(guild);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Update(int id, [FromBody] Guild guild)
        {
            guild.Guildid = id;
            _service.Update(guild);
            return Ok(guild);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            _service.Delete(id);
            return Ok(new { message = "Guild deleted successfully" });
        }
    }
}
