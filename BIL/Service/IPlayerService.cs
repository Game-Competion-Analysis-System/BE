using DAL.Entities;
using DAL.DTO;
using System.Collections.Generic;

namespace BIL.Service
{
    public interface IPlayerService
    {
        List<PlayerDto> GetAll();
        PlayerDto? GetById(int id);
        List<PlayerDto> GetByGame(int gameId);
        List<PlayerDto> GetByServer(int serverId);
        List<PlayerDto> GetByGuild(int guildId);
        List<PlayerDto> SearchByName(string name);
        void Add(Player player);
        void Update(Player player);
        void Delete(int id);
    }
}
