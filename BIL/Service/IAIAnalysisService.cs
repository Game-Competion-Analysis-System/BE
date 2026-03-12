using DAL.DTO;
using DAL.Entities;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BIL.Service
{
    public interface IAIAnalysisService
    {
        Task<AnalysisResultDto?> AnalyzeScreenshotAsync(IFormFile file, int userId);
        Task<List<AnalysisResultDto>> GetHistoryAsync(int userId, string? role);
        Task<Aianalysis?> GetByIdAsync(int id);
        Task<AnalysisResultDto?> GetAnalysisResultAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
