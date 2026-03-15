using DAL.DTO;
using DAL.Entities;
using System.Collections.Generic;

namespace BIL.Service
{
    public interface IServerService
    {
        PagedResult<Server> GetAll(QueryParameters parameters);
        Server? GetById(int id);
        List<Server> GetByGame(int gameId);
        List<Server> SearchByName(string name);
        void Add(Server server);
        void Update(Server server);
        void Delete(int id);
    }
}
