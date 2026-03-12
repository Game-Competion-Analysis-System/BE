using DAL.DTO;
using DAL.Entities;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repository
{  public interface IAIAnalysisRepository
    {
        Task<Aianalysis> ProcessScreenshotAsync(IFormFile file, int userId);
        Task<List<Aianalysis>> GetAllAsync(int? userId = null);
        Task<Aianalysis?> GetByIdAsync(int id);
        Task<Aianalysis?> GetByIdWithDetailsAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
