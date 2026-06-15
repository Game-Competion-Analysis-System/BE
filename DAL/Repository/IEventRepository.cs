using DAL.DTO;
using DAL.Entities;
using System.Collections.Generic;

namespace DAL.Repository
{
    public interface IEventRepository
    {
        List<Event> GetAll(QueryParameters parameters, out int totalCount);
        Event? GetById(int id);
        List<Event> GetByGame(int gameId);
        List<Event> SearchByName(string name);
        void Add(Event @event);
        void Update(Event @event);
        void Delete(int id);
    }
}
