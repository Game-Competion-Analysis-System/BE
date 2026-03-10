using DAL.Entities;
using System.Collections.Generic;

namespace BIL.Service
{
    public interface IServerService
    {
        List<Server> GetAll();
        Server? GetById(int id);
        List<Server> GetByGame(int gameId);
        void Add(Server server);
        void Update(Server server);
        void Delete(int id);
    }
}
