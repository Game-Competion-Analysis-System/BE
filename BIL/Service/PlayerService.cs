using DAL.Entities;
using DAL.Repository;
using System.Collections.Generic;

namespace BIL.Service
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _repo;

        public PlayerService(IPlayerRepository repo)
        {
            _repo = repo;
        }

        public List<Player> GetAll() => _repo.GetAll();
        public Player? GetById(int id) => _repo.GetById(id);
        public List<Player> GetByGame(int gameId) => _repo.GetByGame(gameId);
        public List<Player> GetByServer(int serverId) => _repo.GetByServer(serverId);
        public List<Player> GetByGuild(int guildId) => _repo.GetByGuild(guildId);
        public List<Player> SearchByName(string name) => _repo.SearchByName(name);
        public void Add(Player player) => _repo.Add(player);
        public void Update(Player player) => _repo.Update(player);
        public void Delete(int id) => _repo.Delete(id);
    }
}
