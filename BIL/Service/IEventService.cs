using DAL.DTO;
using DAL.Entities;
using System.Collections.Generic;

namespace BIL.Service
{
    public interface IEventService
    {
        List<EventDto> GetAll();
        Event? GetById(int id);
        List<Event> GetByGame(int gameId);
        List<Event> SearchByName(string name);
        void Add(Event @event);
        void Update(Event @event);
        void Delete(int id);
    }
}
