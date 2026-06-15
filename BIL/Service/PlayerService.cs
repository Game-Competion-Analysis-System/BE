using DAL.Entities;
using DAL.Repository;
using DAL.DTO;
using System.Collections.Generic;
using System.Linq;

namespace BIL.Service
{
    public class PlayerService(IPlayerRepository repo) : IPlayerService
    {
        public PagedResult<PlayerDto> GetAll(QueryParameters parameters)
        {
            var players = repo.GetAll(parameters, out int totalCount);
            return new PagedResult<PlayerDto>
            {
                Items = players.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }
        public PlayerDto? GetById(int id)
        {
            var p = repo.GetById(id);
            return p != null ? MapToDto(p) : null;
        }
        public List<PlayerDto> GetByGame(int gameId) => repo.GetByGame(gameId).Select(MapToDto).ToList();
        public List<PlayerDto> GetByServer(int serverId) => repo.GetByServer(serverId).Select(MapToDto).ToList();
        public List<PlayerDto> GetByGuild(int guildId) => repo.GetByGuild(guildId).Select(MapToDto).ToList();
        public List<PlayerDto> SearchByName(string name) => repo.SearchByName(name).Select(MapToDto).ToList();
        public void Add(Player player) => repo.Add(player);
        public void Update(Player player) => repo.Update(player);
        public void Delete(int id) => repo.Delete(id);

        private static PlayerDto MapToDto(Player p)
        {
            var latestEntry = p.Leaderboardentries.OrderByDescending(e => e.Entryid).FirstOrDefault();
            return new PlayerDto
            {
                PlayerId = p.Playerid,
                PlayerName = p.Playername,
                GuildName = p.Guild?.Guildname,
                GameName = p.Game?.Gamename,
                ServerName = p.Server?.Servername,
                LatestScore = latestEntry?.Value ?? 0,
                LatestRank = latestEntry?.Rank ?? 0
            };
        }
    }
}
