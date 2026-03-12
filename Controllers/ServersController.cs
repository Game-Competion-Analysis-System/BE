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
    public class ServersController(IServerService service) : ControllerBase
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
            var server = service.GetById(id);
            if (server == null) return NotFound();
            return Ok(server);
        }

        [HttpGet("game/{gameId}")]
        [AllowAnonymous]
        public IActionResult GetByGame(int gameId) => Ok(service.GetByGame(gameId));

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Create(Server server)
        {
            service.Add(server);
            return Ok(server);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Update(int id, [FromBody] Server server)
        {
            server.Serverid = id;
            service.Update(server);
            return Ok(server);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            service.Delete(id);
            return Ok(new { message = "Server deleted successfully" });
        }
    }
}
