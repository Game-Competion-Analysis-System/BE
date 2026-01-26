using BIL.Service;
using Microsoft.AspNetCore.Mvc;

namespace GameCompetionAnalysisSystem.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AIController : ControllerBase
    {
        private readonly IAIAnalysisService _service;

        public AIController(IAIAnalysisService service)
        {
            _service = service;
        }

        [HttpPost("analyze")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Analyze(IFormFile file)
        {
            var result = await _service.AnalyzeScreenshotAsync(file);
            return Ok(result);
        }
    }


}
