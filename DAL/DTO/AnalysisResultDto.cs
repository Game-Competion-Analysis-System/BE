using System;
using System.Collections.Generic;

namespace DAL.DTO
{
    public class AnalysisResultDto
    {
        public int AnalysisId { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? ProcessedTime { get; set; }
        public string? GameName { get; set; }
        public string? ServerName { get; set; }
        public string? EventName { get; set; }
        public List<LeaderboardEntryDto> Leaderboard { get; set; } = [];
    }
}
