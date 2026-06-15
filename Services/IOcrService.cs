using GameCompetionAnalysisSystem.Models;

namespace GameCompetionAnalysisSystem.Services
{
    public interface IOcrService
    {
        Task<OcrResult> ExtractTextAsync(IFormFile file, string language = "eng");
    }
}
