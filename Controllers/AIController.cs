using BIL.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameCompetionAnalysisSystem.Controllers
{
    [ApiController]
    [Route("api/ai")]
    [Authorize]
    public class AIController(IAIAnalysisService service) : ControllerBase
    {
        [HttpPost("analyze")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Analyze(IFormFile file)
        {
            var result = await service.AnalyzeScreenshotAsync(file);
            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            return Ok(await service.GetHistoryAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("{id}/result")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAnalysisResult(int id)
        {
            var result = await service.GetAnalysisResultAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }


}
