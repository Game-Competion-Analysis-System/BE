namespace GameCompetionAnalysisSystem.Services
{
    public interface ILeaderboardService
    {
        Task ProcessOcrAsync(int analysisId);
        Task<IEnumerable<object>> GetTopAsync(int n);
    }
}
