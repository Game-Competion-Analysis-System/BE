using DAL.Entities;
using DAL.Repository;
using System.Collections.Generic;

namespace BIL.Service
{
    public class GuildService : IGuildService
    {
        private readonly IGuildRepository _repo;

        public GuildService(IGuildRepository repo)
        {
            _repo = repo;
        }

        public List<Guild> GetAll() => _repo.GetAll();
        public Guild? GetById(int id) => _repo.GetById(id);
        public List<Guild> GetByServer(int serverId) => _repo.GetByServer(serverId);
        public List<Guild> SearchByName(string name) => _repo.SearchByName(name);
        public void Add(Guild guild) => _repo.Add(guild);
        public void Update(Guild guild) => _repo.Update(guild);
        public void Delete(int id) => _repo.Delete(id);
    }
}
