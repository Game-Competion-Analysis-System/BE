using DAL.DTO;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repository
{
    public class PlayerRepository(Swd392GameAiContext context) : IPlayerRepository
    {
        private readonly Swd392GameAiContext _context = context;

        public List<Player> GetAll(QueryParameters parameters, out int totalCount)
        {
            var query = _context.Players
                .Include(p => p.Game)
                .Include(p => p.Server)
                .Include(p => p.Guild)
                .Include(p => p.Leaderboardentries)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                var search = parameters.SearchTerm.ToLower();
                query = query.Where(p => p.Playername != null && p.Playername.ToLower().Contains(search));
            }

            // Filtering
            if (!string.IsNullOrEmpty(parameters.Filter) && int.TryParse(parameters.Filter, out int gameId))
            {
                query = query.Where(p => p.Gameid == gameId);
            }

            totalCount = query.Count();

            // Sorting
            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                switch (parameters.SortBy.ToLower())
                {
                    case "playername":
                        query = parameters.IsDescending ? query.OrderByDescending(p => p.Playername) : query.OrderBy(p => p.Playername);
                        break;
                    default:
                        query = query.OrderBy(p => p.Playerid);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(p => p.Playerid);
            }

            // Paging
            return query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();
        }

        public Player? GetById(int id) => _context.Players
            .Include(p => p.Game)
            .Include(p => p.Server)
            .Include(p => p.Guild)
            .Include(p => p.Leaderboardentries)
            .FirstOrDefault(p => p.Playerid == id);

        public List<Player> GetByGame(int gameId) => _context.Players
            .Include(p => p.Game)
            .Include(p => p.Server)
            .Include(p => p.Guild)
            .Include(p => p.Leaderboardentries)
            .Where(p => p.Gameid == gameId)
            .ToList();

        public List<Player> GetByServer(int serverId) => _context.Players
            .Include(p => p.Game)
            .Include(p => p.Server)
            .Include(p => p.Guild)
            .Include(p => p.Leaderboardentries)
            .Where(p => p.Serverid == serverId)
            .ToList();

        public List<Player> GetByGuild(int guildId) => _context.Players
            .Include(p => p.Game)
            .Include(p => p.Server)
            .Include(p => p.Guild)
            .Include(p => p.Leaderboardentries)
            .Where(p => p.Guildid == guildId)
            .ToList();

        public List<Player> SearchByName(string name)
        {
            var pattern = $"%{name.Replace(" ", "%").Replace("-", "%")}%";
            return _context.Players
                .Include(p => p.Game)
                .Include(p => p.Server)
                .Include(p => p.Guild)
                .Include(p => p.Leaderboardentries)
                .Where(p => p.Playername != null && EF.Functions.ILike(p.Playername, pattern))
                .ToList();
        }

        public void Add(Player player)
        {
            _context.Players.Add(player);
            _context.SaveChanges();
        }

        public void Update(Player player)
        {
            _context.Players.Update(player);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var player = _context.Players.Find(id);
            if (player != null)
            {
                _context.Players.Remove(player);
                _context.SaveChanges();
            }
        }
    }
}
