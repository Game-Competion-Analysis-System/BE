using DAL.DTO;
using DAL.Entities;
using DAL.Repository;
using System.Collections.Generic;

namespace BIL.Service
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _repo;

        public EventService(IEventRepository repo)
        {
            _repo = repo;
        }

        public List<EventDto> GetAll() => _repo.GetAll().Select(e => new EventDto
        {
            EventId = e.Eventid,
            EventName = e.Eventname,
            EventType = e.Eventtype,
            StartDate = e.Startdate,
            EndDate = e.Enddate,
            GameName = e.Game?.Gamename
        }).ToList();
        public Event? GetById(int id) => _repo.GetById(id);
        public List<Event> GetByGame(int gameId) => _repo.GetByGame(gameId);
        public List<Event> SearchByName(string name) => _repo.SearchByName(name);
        public void Add(Event @event) => _repo.Add(@event);
        public void Update(Event @event) => _repo.Update(@event);
        public void Delete(int id) => _repo.Delete(id);
    }
}
