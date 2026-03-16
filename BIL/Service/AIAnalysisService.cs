using DAL.Entities;
using DAL.Repository;
using DAL.DTO;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BIL.Service
{
    public class AIAnalysisService(IAIAnalysisRepository repo) : IAIAnalysisService
    {
        public async Task<AnalysisResultDto?> AnalyzeScreenshotAsync(IFormFile file, int userId, string gameName)
        {
            var analysis = await repo.ProcessScreenshotAsync(file, userId, gameName);
            if (analysis == null) return null;

            return await GetAnalysisResultAsync(analysis.Analysisid);
        }

        public async Task<AnalysisResultDto?> AnalyzeLatestFromCloudAsync(int userId, string gameName)
        {
            var analysis = await repo.ProcessLatestImageFromCloudAsync(userId, gameName);
            if (analysis == null) return null;

            return await GetAnalysisResultAsync(analysis.Analysisid);
        }

        public async Task<PagedResult<AnalysisResultDto>> GetHistoryAsync(int userId, string? role, QueryParameters parameters)
        {
            int? filterUserId = (role?.ToLower() == "admin") ? null : userId;
            var (analyses, totalCount) = await repo.GetAllAsync(parameters, filterUserId);
            List<AnalysisResultDto> results = [];
            foreach (var a in analyses)
            {
                var result = await GetAnalysisResultAsync(a.Analysisid);
                if (result != null)
                {
                    results.Add(result);
                }
            }
            return new PagedResult<AnalysisResultDto>
            {
                Items = results,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        public async Task<AnalysisResultDto?> GetByIdAsync(int id)
        {
            return await GetAnalysisResultAsync(id);
        }

        public async Task<AnalysisResultDto?> GetAnalysisResultAsync(int id)
        {
            var analysis = await repo.GetByIdWithDetailsAsync(id);
            if (analysis == null) return null;

            var leaderboard = analysis.Leaderboards.FirstOrDefault();
            var eventObj = leaderboard?.Event;
            
            // Get GameName and ServerName from extracted fields first (prioritize user input or high confidence AI)
            var gameName = analysis.Aiextractedfields
                .Where(f => f.Fieldtype == "GameName")
                .OrderByDescending(f => f.Confidence)
                .Select(f => f.Rawtext)
                .FirstOrDefault();

            var serverName = analysis.Aiextractedfields
                .Where(f => f.Fieldtype == "ServerName")
                .OrderByDescending(f => f.Confidence)
                .Select(f => f.Rawtext)
                .FirstOrDefault();

            // Fallback to linked entities if not in extracted fields
            if (string.IsNullOrEmpty(gameName))
            {
                gameName = eventObj?.Game?.Gamename;
            }
            if (string.IsNullOrEmpty(serverName))
            {
                // Try to get server name from the first player in the leaderboard
                serverName = leaderboard?.Leaderboardentries.FirstOrDefault()?.Player?.Server?.Servername;
            }

            var result = new AnalysisResultDto
            {
                AnalysisId = analysis.Analysisid,
                ImageUrl = analysis.Upload?.Imageurl,
                ProcessedTime = analysis.Processedtime,
                GameName = gameName,
                EventName = eventObj?.Eventname,
                ServerName = serverName,
                Leaderboard = []
            };

            if (leaderboard != null)
            {
                result.Leaderboard = leaderboard.Leaderboardentries
                    .OrderBy(e => e.Rank)
                    .Select(e => new LeaderboardEntryDto
                    {
                        Rank = e.Rank ?? 0,
                        PlayerName = e.Player?.Playername ?? "Unknown",
                        Score = e.Value ?? 0,
                        Value = e.Value ?? 0,
                        GuildName = e.Player?.Guild?.Guildname
                    })
                    .ToList();
            }

            return result;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await repo.DeleteAsync(id);
        }

        public async Task<List<string>> GetAirtestUploadImagesAsync()
        {
            return await repo.GetAirtestUploadImagesAsync();
        }
    }
}
