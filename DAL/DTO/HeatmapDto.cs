using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DAL.DTO
{
    public class HeatmapDto
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
        public List<HeatmapPlayerDto> Players { get; set; } = [];
    }

    public class HeatmapPlayerDto
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }
}
