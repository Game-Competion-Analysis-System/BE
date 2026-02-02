using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BIL.Service
{
    public interface IAIAnalysisService
    {
        Task<Aianalysis> AnalyzeScreenshotAsync(IFormFile file);

    }
}
