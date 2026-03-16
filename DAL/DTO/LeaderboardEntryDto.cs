using System.Collections.Generic;

namespace DAL.DTO
{
    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public string? PlayerName { get; set; }
        public double Score { get; set; }
        public double Value { get; set; } // Alias for FE compatibility
        public string? GuildName { get; set; }
    }
}
