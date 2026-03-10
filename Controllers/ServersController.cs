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
        private readonly IServerService _service = service;

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll() => Ok(_service.GetAll());

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetById(int id)
        {
            var server = _service.GetById(id);
            if (server == null) return NotFound();
            return Ok(server);
        }

        [HttpGet("game/{gameId}")]
        [AllowAnonymous]
        public IActionResult GetByGame(int gameId) => Ok(_service.GetByGame(gameId));

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Create(Server server)
        {
            _service.Add(server);
            return Ok(server);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Update(int id, [FromBody] Server server)
        {
            server.Serverid = id;
            _service.Update(server);
            return Ok(server);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            _service.Delete(id);
            return Ok(new { message = "Server deleted successfully" });
        }
    }
}
