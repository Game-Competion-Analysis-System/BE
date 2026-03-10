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

        public List<Event> GetAll() => _repo.GetAll();
        public Event? GetById(int id) => _repo.GetById(id);
        public List<Event> GetByGame(int gameId) => _repo.GetByGame(gameId);
        public void Add(Event @event) => _repo.Add(@event);
        public void Update(Event @event) => _repo.Update(@event);
        public void Delete(int id) => _repo.Delete(id);
    }
}
