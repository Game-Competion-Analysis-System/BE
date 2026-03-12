using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIL.Service
{
    public interface ILeaderboardService
    {
        Task ProcessOcrAsync(int analysisId);
        Task<List<Leaderboardentry>> GetTopAsync(int n);
        Task<List<Leaderboard>> GetAllAsync();
        Task<Leaderboard?> GetByIdAsync(int id);
        Task<List<Leaderboardentry>> GetEntriesByLeaderboardIdAsync(int leaderboardId);
        Task<List<Leaderboardentry>> GetSortedEntriesByLeaderboardIdAsync(int leaderboardId);
        Task DeleteAsync(int id);
    }

}
