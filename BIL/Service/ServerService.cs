using DAL.DTO;
using DAL.Entities;
using DAL.Repository;
using System.Collections.Generic;

namespace BIL.Service
{
    public class ServerService(IServerRepository repo) : IServerService
    {
        public PagedResult<Server> GetAll(QueryParameters parameters)
        {
            var servers = repo.GetAll(parameters, out int totalCount);
            return new PagedResult<Server>
            {
                Items = servers,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }
        public Server? GetById(int id) => repo.GetById(id);
        public List<Server> GetByGame(int gameId) => repo.GetByGame(gameId);
        public List<Server> SearchByName(string name) => repo.SearchByName(name);
        public void Add(Server server) => repo.Add(server);
        public void Update(Server server) => repo.Update(server);
        public void Delete(int id) => repo.Delete(id);
    }
}
