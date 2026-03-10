using DAL.Entities;
using DAL.Repository;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BIL.Service
{
    public class AIAnalysisService : IAIAnalysisService
    {
        private readonly IAIAnalysisRepository _repo;

        public AIAnalysisService(IAIAnalysisRepository repo)
        {
            _repo = repo;
        }

        public Task<Aianalysis> AnalyzeScreenshotAsync(IFormFile file)
        {
            return _repo.ProcessScreenshotAsync(file);
        }

        public async Task<List<Aianalysis>> GetHistoryAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Aianalysis?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }
    }


}
