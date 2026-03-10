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
    public class EventsController(IEventService service) : ControllerBase
    {
        private readonly IEventService _service = service;

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll() => Ok(_service.GetAll());

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetById(int id)
        {
            var @event = _service.GetById(id);
            if (@event == null) return NotFound();
            return Ok(@event);
        }

        [HttpGet("game/{gameId}")]
        [AllowAnonymous]
        public IActionResult GetByGame(int gameId) => Ok(_service.GetByGame(gameId));

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Create(Event @event)
        {
            _service.Add(@event);
            return Ok(@event);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Update(int id, [FromBody] Event @event)
        {
            @event.Eventid = id;
            _service.Update(@event);
            return Ok(@event);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            _service.Delete(id);
            return Ok(new { message = "Event deleted successfully" });
        }
    }
}
