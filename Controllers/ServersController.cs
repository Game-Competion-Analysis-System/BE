using Microsoft.EntityFrameworkCore;
using BIL.Service;
using DAL.DTO;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameCompetionAnalysisSystem.Controllers
{
    [ApiController]
    [Route("api/servers")]
    [Authorize]
    public class ServersController(IServerService service) : ControllerBase
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
            return Ok(new ServerDto
            {
                ServerId = server.Serverid,
                ServerName = server.Servername,
                Region = server.Region,
                Status = server.Status
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Update(int id, [FromBody] Server server)
        {
            server.Serverid = id;
            service.Update(server);
            return Ok(new ServerDto
            {
                ServerId = server.Serverid,
                ServerName = server.Servername,
                Region = server.Region,
                Status = server.Status
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            service.Delete(id);
            return Ok(new { message = "Delete successful" });
        }
    }
}
