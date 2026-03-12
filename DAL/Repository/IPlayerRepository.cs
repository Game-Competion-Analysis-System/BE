using DAL.DTO;
using DAL.Entities;
using System.Collections.Generic;

namespace DAL.Repository
{
    public interface IPlayerRepository
    {
        List<Player> GetAll();
        Player? GetById(int id);
        List<Player> GetByGame(int gameId);
        List<Player> GetByServer(int serverId);
        List<Player> GetByGuild(int guildId);
        List<Player> SearchByName(string name);
        void Add(Player player);
        void Update(Player player);
        void Delete(int id);
    }
}
