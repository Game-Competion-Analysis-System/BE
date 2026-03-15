using DAL.DTO;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class GameRepository(Swd392GameAiContext context) : IGameRepository
    {
        private readonly Swd392GameAiContext _context = context;

        public List<Game> GetAll(QueryParameters parameters, out int totalCount)
        {
            var query = _context.Games.Include(g => g.Company).AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                var search = parameters.SearchTerm.ToLower();
                query = query.Where(g => 
                    (g.Gamename != null && g.Gamename.ToLower().Contains(search)) || 
                    (g.Genre != null && g.Genre.ToLower().Contains(search)));
            }

            // Filtering
            if (!string.IsNullOrEmpty(parameters.Filter))
            {
                var filter = parameters.Filter.ToLower();
                query = query.Where(g => g.Genre != null && g.Genre.ToLower() == filter);
            }

            totalCount = query.Count();

            // Sorting
            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                switch (parameters.SortBy.ToLower())
                {
                    case "gamename":
                        query = parameters.IsDescending ? query.OrderByDescending(g => g.Gamename) : query.OrderBy(g => g.Gamename);
                        break;
                    case "genre":
                        query = parameters.IsDescending ? query.OrderByDescending(g => g.Genre) : query.OrderBy(g => g.Genre);
                        break;
                    default:
                        query = query.OrderBy(g => g.Gameid);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(g => g.Gameid);
            }

            // Paging
            return query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();
        }


        public List<Game> GetMMORPG()
            => _context.Games
                .Where(g => g.Genre != null && g.Genre.Contains("MMORPG"))
                .ToList();

        public List<Game> SearchByName(string name)
        {
            var pattern = $"%{name.Replace(" ", "%").Replace("-", "%")}%";
            return _context.Games
                .Where(g => g.Gamename != null && EF.Functions.ILike(g.Gamename, pattern))
                .ToList();
        }

        public Game? GetById(int id)
        {
            return _context.Games
                .Include(g => g.Company)
                .Include(g => g.Events)
                .Include(g => g.Players)
                .FirstOrDefault(g => g.Gameid == id);
        }
        public void Add(Game game)
        {
            _context.Games.Add(game);
            _context.SaveChanges();
        }

        public void Update(Game game)
        {
            _context.Games.Update(game);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var game = _context.Games
                .Include(g => g.Events)
                    .ThenInclude(e => e.Leaderboards)
                        .ThenInclude(lb => lb.Leaderboardentries)
                .Include(g => g.Servers)
                    .ThenInclude(s => s.Guilds)
                .Include(g => g.Players)
                .FirstOrDefault(g => g.Gameid == id);

            if (game != null)
            {
                // 1. Delete LeaderboardEntries first (Fix for fk_entry_player)
                if (game.Events != null)
                {
                    foreach (var ev in game.Events)
                    {
                        if (ev.Leaderboards != null)
                        {
                            foreach (var lb in ev.Leaderboards)
                            {
                                if (lb.Leaderboardentries != null && lb.Leaderboardentries.Count > 0)
                                    _context.Leaderboardentries.RemoveRange(lb.Leaderboardentries);
                            }
                            _context.Leaderboards.RemoveRange(ev.Leaderboards);
                        }
                    }
                    _context.Events.RemoveRange(game.Events);
                }

                // 2. Delete Guilds before Servers
                if (game.Servers != null)
                {
                    foreach (var s in game.Servers)
                    {
                        if (s.Guilds != null && s.Guilds.Count > 0) _context.Guilds.RemoveRange(s.Guilds);
                    }
                    _context.Servers.RemoveRange(game.Servers);
                }

                // 3. Delete Players
                if (game.Players != null && game.Players.Count > 0) _context.Players.RemoveRange(game.Players);

                // 4. Finally Delete Game
                _context.Games.Remove(game);
                _context.SaveChanges();
            }
        }

    }
}
