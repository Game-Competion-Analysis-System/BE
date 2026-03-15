using DAL.DTO;
using DAL.Entities;
using DAL.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BIL.Service
{
    public class LeaderboardService(ILeaderboardRepository repo) : ILeaderboardService
    {
        public async Task ProcessOcrAsync(int analysisId)
        {
            await repo.ParseOcrAndSaveAsync(analysisId);
        }

        public async Task<List<Leaderboardentry>> GetTopAsync(int n)
        {
            return await repo.GetTopAsync(n);
        }

        public async Task<PagedResult<Leaderboard>> GetAllAsync(QueryParameters parameters)
        {
            var (items, totalCount) = await repo.GetAllAsync(parameters);
            return new PagedResult<Leaderboard>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        public async Task<Leaderboard?> GetByIdAsync(int id)
        {
            return await repo.GetByIdAsync(id);
        }

        public async Task<List<Leaderboardentry>> GetEntriesByLeaderboardIdAsync(int leaderboardId)
        {
            return await repo.GetEntriesByLeaderboardIdAsync(leaderboardId);
        }

        public async Task<List<Leaderboardentry>> GetSortedEntriesByLeaderboardIdAsync(int leaderboardId)
        {
            return await repo.GetSortedEntriesByLeaderboardIdAsync(leaderboardId);
        }

        public async Task DeleteAsync(int id)
        {
            await repo.DeleteAsync(id);
        }
    }


}
