using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DAL.Repository
{
    public class LeaderboardRepository : ILeaderboardRepository
    {
        private readonly Swd392GameAiContext _context;

        public LeaderboardRepository(Swd392GameAiContext context)
        {
            _context = context;
        }

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

        public async Task<List<Leaderboard>> GetAllAsync()
        {
            return await _context.Leaderboards.ToListAsync();
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
