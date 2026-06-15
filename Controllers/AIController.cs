using GameCompetionAnalysisSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameCompetionAnalysisSystem.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AIController : ControllerBase
    {
        private readonly IOcrService _ocrService;

        public AIController(IOcrService ocrService)
        {
            _ocrService = ocrService;
        }

        /// <summary>
        /// Extract text from an image using OCR.
        /// </summary>
        /// <param name="file">Image file (png, jpg, etc.)</param>
        /// <param name="language">OCR language: "eng" (default) or "vie"</param>
        [HttpPost("analyze")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Analyze(IFormFile file, [FromQuery] string language = "eng")
        {
            if (file == null || file.Length == 0)
                return BadRequest("No image file provided.");

            if (language != "eng" && language != "vie")
                return BadRequest("Language must be 'eng' or 'vie'.");

            var result = await _ocrService.ExtractTextAsync(file, language);

            if (!result.Success)
                return StatusCode(502, new { error = "OCR service returned an unsuccessful response." });

            return Ok(result);
        }
    }
}
