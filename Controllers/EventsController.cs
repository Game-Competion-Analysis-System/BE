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
    public class EventsController(IEventService service) : ControllerBase
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
            var @event = service.GetById(id);
            if (@event == null) return NotFound();
            return Ok(@event);
        }

        [HttpGet("game/{gameId}")]
        [AllowAnonymous]
        public IActionResult GetByGame(int gameId) => Ok(service.GetByGame(gameId));

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Create(Event @event)
        {
            service.Add(@event);
            return Ok(new EventDto
            {
                EventId = @event.Eventid,
                EventName = @event.Eventname,
                EventType = @event.Eventtype,
                StartDate = @event.Startdate,
                EndDate = @event.Enddate
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Update(int id, [FromBody] Event @event)
        {
            @event.Eventid = id;
            service.Update(@event);
            return Ok(new EventDto
            {
                EventId = @event.Eventid,
                EventName = @event.Eventname,
                EventType = @event.Eventtype,
                StartDate = @event.Startdate,
                EndDate = @event.Enddate
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            service.Delete(id);
            return Ok(new { message = "Event deleted successfully" });
        }
    }
}
