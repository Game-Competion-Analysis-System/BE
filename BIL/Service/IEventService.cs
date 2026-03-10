using DAL.Entities;
using System.Collections.Generic;

namespace BIL.Service
{
    public interface IEventService
    {
        List<Event> GetAll();
        Event? GetById(int id);
        List<Event> GetByGame(int gameId);
        void Add(Event @event);
        void Update(Event @event);
        void Delete(int id);
    }
}
