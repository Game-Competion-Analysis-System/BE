using BIL.Service;
using DAL.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GameCompetionAnalysisSystem.Controllers
{
    public enum SupportedGame
    {
        VLTK_Mobile,
        VLTK_2_0
    }

    [ApiController]
    [Route("api/ai")]
    [Authorize]
    public class AIController(IAIAnalysisService service) : ControllerBase
    {
        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeScreenshot(IFormFile file, [FromQuery] SupportedGame gameName)
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            string gameNameStr = gameName == SupportedGame.VLTK_Mobile ? "VLTK Mobile" : "VLTK 2.0";
            var result = await service.AnalyzeScreenshotAsync(file, userId, gameNameStr);
            if (result == null) return BadRequest("Analysis failed");

            return Ok(result);
        }

        [HttpPost("analyze/automatic")]
        public async Task<IActionResult> AnalyzeAutomatic([FromQuery] SupportedGame gameName)
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            string gameNameStr = gameName == SupportedGame.VLTK_Mobile ? "VLTK Mobile" : "VLTK 2.0";
            var result = await service.AnalyzeLatestFromCloudAsync(userId, gameNameStr);
            if (result == null) return NotFound(new { message = "No new image found in Cloudinary folder 'AirtestUpload' to process." });

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] QueryParameters parameters)
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(await service.GetHistoryAsync(userId, role, parameters));
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

        [HttpGet("airtest-uploads")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAirtestUploads()
        {
            var urls = await service.GetAirtestUploadImagesAsync();
            return Ok(urls);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await service.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
