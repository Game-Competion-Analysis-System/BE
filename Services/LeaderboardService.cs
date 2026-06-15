namespace GameCompetionAnalysisSystem.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly List<object> _entries = new();

        public Task ProcessOcrAsync(int analysisId)
        {
            // Placeholder: wire up real OCR-to-leaderboard parsing when DB is connected
            return Task.CompletedTask;
        }

        public Task<IEnumerable<object>> GetTopAsync(int n) =>
            Task.FromResult(_entries.Take(n));
    }
}
