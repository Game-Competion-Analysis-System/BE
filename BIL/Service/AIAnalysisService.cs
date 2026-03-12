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
        public Task<Aianalysis> AnalyzeScreenshotAsync(IFormFile file)
        {
            return repo.ProcessScreenshotAsync(file);
        }

        public async Task<List<Aianalysis>> GetHistoryAsync()
        {
            return await repo.GetAllAsync();
        }

        public async Task<Aianalysis?> GetByIdAsync(int id)
        {
            return await repo.GetByIdAsync(id);
        }

        public async Task<AnalysisResultDto?> GetAnalysisResultAsync(int id)
        {
            var analysis = await repo.GetByIdWithDetailsAsync(id);
            if (analysis == null) return null;

            var leaderboard = analysis.Leaderboards.FirstOrDefault();
            var eventObj = leaderboard?.Event;
            var game = eventObj?.Game;
            var server = game?.Servers.FirstOrDefault(); // Simplified logic

            var result = new AnalysisResultDto
            {
                AnalysisId = analysis.Analysisid,
                ImageUrl = analysis.Upload?.Imageurl,
                ProcessedTime = analysis.Processedtime,
                GameName = game?.Gamename,
                EventName = eventObj?.Eventname,
                ServerName = server?.Servername,
                Leaderboard = []
            };

            if (leaderboard != null)
            {
                result.Leaderboard = leaderboard.Leaderboardentries
                    .OrderBy(e => e.Rank)
                    .Select(e => new LeaderboardEntryDto
                    {
                        Rank = e.Rank ?? 0,
                        PlayerName = e.Player?.Playername,
                        Score = e.Value ?? 0,
                        GuildName = e.Player?.Guild?.Guildname
                    })
                    .ToList();
            }

            return result;
        }
    }
}
