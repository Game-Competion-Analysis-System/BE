using DAL.DTO;
using DAL.Entities;
using System.Collections.Generic;

namespace BIL.Service
{
    public interface IEventService
    {
        PagedResult<EventDto> GetAll(QueryParameters parameters);
        EventDto? GetById(int id);
        List<EventDto> GetByGame(int gameId);
        List<EventDto> SearchByName(string name);
        void Add(Event @event);
        void Update(Event @event);
        void Delete(int id);
    }
}
