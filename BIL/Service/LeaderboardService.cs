using DAL.Entities;
using DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIL.Service
{
    public class LeaderboardService(ILeaderboardRepository repo) : ILeaderboardService
    {
        private readonly ILeaderboardRepository _repo = repo;

        public async Task ProcessOcrAsync(int analysisId)
        {
            await _repo.ParseOcrAndSaveAsync(analysisId);
        }

        public async Task<List<Leaderboardentry>> GetTopAsync(int n)
        {
            return await _repo.GetTopAsync(n);
        }

        public async Task<List<Leaderboard>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Leaderboard?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<List<Leaderboardentry>> GetEntriesByLeaderboardIdAsync(int leaderboardId)
        {
            return await _repo.GetEntriesByLeaderboardIdAsync(leaderboardId);
        }

        public async Task<List<Leaderboardentry>> GetSortedEntriesByLeaderboardIdAsync(int leaderboardId)
        {
            return await _repo.GetSortedEntriesByLeaderboardIdAsync(leaderboardId);
        }

        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }
    }


}
