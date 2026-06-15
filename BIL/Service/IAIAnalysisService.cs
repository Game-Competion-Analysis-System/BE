using DAL.DTO;
using DAL.Entities;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BIL.Service
{
    public interface IAIAnalysisService
    {
        Task<AnalysisResultDto?> AnalyzeScreenshotAsync(IFormFile file, int userId, string gameName);
        Task<AnalysisResultDto?> AnalyzeLatestFromCloudAsync(int userId, string gameName);
        Task<PagedResult<AnalysisResultDto>> GetHistoryAsync(int userId, string? role, AIQueryParameters parameters);
        Task<AnalysisResultDto?> GetByIdAsync(int id);
        Task<AnalysisResultDto?> GetAnalysisResultAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<List<string>> GetAirtestUploadImagesAsync();
        Task<List<HeatmapDto>> GetHeatmapDataAsync(int userId, string? role);
    }
}
