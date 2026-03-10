using DAL.Entities;
using DAL.Repository;
using System.Collections.Generic;

namespace BIL.Service
{
    public class ServerService : IServerService
    {
        private readonly IServerRepository _repo;

        public ServerService(IServerRepository repo)
        {
            _repo = repo;
        }

        public List<Server> GetAll() => _repo.GetAll();
        public Server? GetById(int id) => _repo.GetById(id);
        public List<Server> GetByGame(int gameId) => _repo.GetByGame(gameId);
        public void Add(Server server) => _repo.Add(server);
        public void Update(Server server) => _repo.Update(server);
        public void Delete(int id) => _repo.Delete(id);
    }
}
