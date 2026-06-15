using DAL.DTO;
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
        Task<List<LeaderboardEntryDto>> GetTopAsync(int n);
        Task<PagedResult<LeaderboardDto>> GetAllAsync(QueryParameters parameters);
        Task<LeaderboardDto?> GetByIdAsync(int id);
        Task<List<LeaderboardEntryDto>> GetEntriesByLeaderboardIdAsync(int leaderboardId);
        Task<List<LeaderboardEntryDto>> GetSortedEntriesByLeaderboardIdAsync(int leaderboardId);
        Task DeleteAsync(int id);
    }

}
