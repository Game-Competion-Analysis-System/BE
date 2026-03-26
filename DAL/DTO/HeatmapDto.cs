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
    }

    public class HeatmapPlayerDtoComparer : IEqualityComparer<HeatmapPlayerDto>
    {
        public bool Equals(HeatmapPlayerDto? x, HeatmapPlayerDto? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.PlayerId == y.PlayerId;
        }

        public int GetHashCode([DisallowNull] HeatmapPlayerDto obj)
        {
            return obj.PlayerId.GetHashCode();
        }
    }
}
