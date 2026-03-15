using DAL.DTO;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class LeaderboardRepository(Swd392GameAiContext context) : ILeaderboardRepository
    {
        private readonly Swd392GameAiContext _context = context;

        public async Task ParseOcrAndSaveAsync(int analysisId)
        {
            var players = await _context.Players
                .OrderBy(p => p.Playerid)
                .Take(10)
                .ToListAsync();

            int rank = 1;

            foreach (var player in players)
            {
                _context.Leaderboardentries.Add(new Leaderboardentry
                {
                    Rank = rank++,
                    Playerid = player.Playerid,
                    Leaderboardid = null,
                    Value = 0
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Leaderboardentry>> GetTopAsync(int n)
        {
            return await _context.Leaderboardentries
                .Include(x => x.Player)
                .OrderBy(x => x.Rank)
                .Take(n)
                .ToListAsync();
        }

        public async Task<(List<Leaderboard> Items, int TotalCount)> GetAllAsync(QueryParameters parameters)
        {
            var query = _context.Leaderboards
                .Include(l => l.Event)
                .Include(l => l.Createdfromanalysis)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                var search = parameters.SearchTerm.ToLower();
                query = query.Where(l => l.Title != null && l.Title.ToLower().Contains(search));
            }

            // Filtering
            if (!string.IsNullOrEmpty(parameters.Filter))
            {
                var filter = parameters.Filter.ToLower();
                query = query.Where(l => l.Metrictype != null && l.Metrictype.ToLower() == filter);
            }

            var totalCount = await query.CountAsync();

            // Sorting
            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                switch (parameters.SortBy.ToLower())
                {
                    case "title":
                        query = parameters.IsDescending ? query.OrderByDescending(l => l.Title) : query.OrderBy(l => l.Title);
                        break;
                    default:
                        query = query.OrderBy(l => l.Leaderboardid);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(l => l.Leaderboardid);
            }

            // Paging
            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Leaderboard?> GetByIdAsync(int id)
        {
            return await _context.Leaderboards.FindAsync(id);
        }

        public async Task<List<Leaderboardentry>> GetEntriesByLeaderboardIdAsync(int leaderboardId)
        {
            return await _context.Leaderboardentries
                .Include(x => x.Player)
                .Where(x => x.Leaderboardid == leaderboardId)
                .OrderBy(x => x.Rank)
                .ToListAsync();
        }

        public async Task<List<Leaderboardentry>> GetSortedEntriesByLeaderboardIdAsync(int leaderboardId)
        {
            return await _context.Leaderboardentries
                .Include(x => x.Player)
                .Where(x => x.Leaderboardid == leaderboardId)
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Rank)
                .ToListAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var lb = await _context.Leaderboards.FindAsync(id);
            if (lb != null)
            {
                _context.Leaderboards.Remove(lb);
                await _context.SaveChangesAsync();
            }
        }
    }
}
