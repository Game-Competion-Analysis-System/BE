using DAL.DTO;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface ILeaderboardRepository
    {
        Task ParseOcrAndSaveAsync(int analysisId);
        Task<List<Leaderboardentry>> GetTopAsync(int n);
        Task<(List<Leaderboard> Items, int TotalCount)> GetAllAsync(QueryParameters parameters);
        Task<Leaderboard?> GetByIdAsync(int id);
        Task<List<Leaderboardentry>> GetEntriesByLeaderboardIdAsync(int leaderboardId);
        Task<List<Leaderboardentry>> GetSortedEntriesByLeaderboardIdAsync(int leaderboardId);
        Task DeleteAsync(int id);
    }
}
