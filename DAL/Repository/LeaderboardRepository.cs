using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DAL.Repository
{
    public class LeaderboardRepository : ILeaderboardRepository
    {
        private readonly PostgresContext _context;

        public LeaderboardRepository(PostgresContext context)
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
    }
}
