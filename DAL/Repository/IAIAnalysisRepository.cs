using DAL.DTO;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DAL.Repository
{
    public interface IAIAnalysisRepository
    {
        Task<Aianalysis> ProcessScreenshotAsync(IFormFile file, int userId, string gameName);
        Task<Aianalysis?> ProcessLatestImageFromCloudAsync(int userId, string gameName);
        Task<(List<Aianalysis> Items, int TotalCount)> GetAllAsync(AIQueryParameters parameters, int? userId = null);
        Task<Aianalysis?> GetByIdAsync(int id);
        Task<Aianalysis?> GetByIdWithDetailsAsync(int id);
        Task<bool> DeleteAsync(int id);
         Task<List<string>> GetAirtestUploadImagesAsync();
         Task<List<HeatmapDto>> GetHeatmapDataAsync(int? userId = null);
     }


}
