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
        Task<Aianalysis> ProcessScreenshotAsync(IFormFile file);
    }


}
