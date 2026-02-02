using DAL.Entities;
using DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIL.Service
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ILeaderboardRepository _repo;

        public LeaderboardService(ILeaderboardRepository repo)
        {
            _repo = repo;
        }

        public async Task ProcessOcrAsync(int analysisId)
        {
            await _repo.ParseOcrAndSaveAsync(analysisId);
        }

        public async Task<List<Leaderboardentry>> GetTopAsync(int n)
        {
            return await _repo.GetTopAsync(n);
        }
    }


}
