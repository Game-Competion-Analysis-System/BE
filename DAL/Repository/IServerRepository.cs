using DAL.Entities;
using System.Collections.Generic;

namespace DAL.Repository
{
    public interface IServerRepository
    {
        List<Server> GetAll();
        Server? GetById(int id);
        List<Server> GetByGame(int gameId);
        List<Server> SearchByName(string name);
        void Add(Server server);
        void Update(Server server);
        void Delete(int id);
    }
}
