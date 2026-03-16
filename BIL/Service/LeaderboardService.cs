using DAL.DTO;
using DAL.Entities;
using DAL.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BIL.Service
{
    public class LeaderboardService(ILeaderboardRepository repo) : ILeaderboardService
    {
        public async Task ProcessOcrAsync(int analysisId)
        {
            await repo.ParseOcrAndSaveAsync(analysisId);
        }

        public async Task<List<LeaderboardEntryDto>> GetTopAsync(int n)
        {
            var entries = await repo.GetTopAsync(n);
            return entries.Select(e => new LeaderboardEntryDto
            {
                Rank = e.Rank ?? 0,
                PlayerName = e.Player?.Playername,
                Score = e.Value ?? 0,
                GuildName = e.Player?.Guild?.Guildname
            }).ToList();
        }

        public async Task<PagedResult<LeaderboardDto>> GetAllAsync(QueryParameters parameters)
        {
            var (items, totalCount) = await repo.GetAllAsync(parameters);
            return new PagedResult<LeaderboardDto>
            {
                Items = items.Select(l => new LeaderboardDto
                {
                    LeaderboardId = l.Leaderboardid,
                    Title = l.Title,
                    EventName = l.Event?.Eventname,
                    MetricType = l.Metrictype
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        public async Task<LeaderboardDto?> GetByIdAsync(int id)
        {
            var l = await repo.GetByIdAsync(id);
            return l == null ? null : new LeaderboardDto
            {
                LeaderboardId = l.Leaderboardid,
                Title = l.Title,
                EventName = l.Event?.Eventname,
                MetricType = l.Metrictype
            };
        }

        public async Task<List<LeaderboardEntryDto>> GetEntriesByLeaderboardIdAsync(int leaderboardId)
        {
            var entries = await repo.GetEntriesByLeaderboardIdAsync(leaderboardId);
            return entries.Select(e => new LeaderboardEntryDto
            {
                Rank = e.Rank ?? 0,
                PlayerName = e.Player?.Playername,
                Score = e.Value ?? 0,
                GuildName = e.Player?.Guild?.Guildname
            }).ToList();
        }

        public async Task<List<LeaderboardEntryDto>> GetSortedEntriesByLeaderboardIdAsync(int leaderboardId)
        {
            var entries = await repo.GetSortedEntriesByLeaderboardIdAsync(leaderboardId);
            return entries.Select(e => new LeaderboardEntryDto
            {
                Rank = e.Rank ?? 0,
                PlayerName = e.Player?.Playername,
                Score = e.Value ?? 0,
                GuildName = e.Player?.Guild?.Guildname
            }).ToList();
        }

        public async Task DeleteAsync(int id)
        {
            await repo.DeleteAsync(id);
        }
    }


}
