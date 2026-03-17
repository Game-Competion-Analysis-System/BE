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
        [AllowAnonymous]
        public async Task<IActionResult> AnalyzeScreenshot(IFormFile file, [FromQuery] SupportedGame gameName)
        {
            try
            {
                var userIdStr = User.FindFirst("UserId")?.Value;
                int userId = 1; // Default to system user if not logged in
                if (!string.IsNullOrEmpty(userIdStr)) int.TryParse(userIdStr, out userId);

                string gameNameStr = gameName == SupportedGame.VLTK_Mobile ? "VLTK Mobile" : "VLTK 2.0";
                var result = await service.AnalyzeScreenshotAsync(file, userId, gameNameStr);
                if (result == null) return BadRequest(new { message = "Failed to process image." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpPost("analyze/automatic")]
        [AllowAnonymous]
        public async Task<IActionResult> AnalyzeAutomatic([FromQuery] SupportedGame gameName)
        {
            try
            {
                var userIdStr = User.FindFirst("UserId")?.Value;
                int userId = 1; // Default to system user if not logged in
                if (!string.IsNullOrEmpty(userIdStr)) int.TryParse(userIdStr, out userId);

                string gameNameStr = gameName == SupportedGame.VLTK_Mobile ? "VLTK Mobile" : "VLTK 2.0";
                var result = await service.AnalyzeLatestFromCloudAsync(userId, gameNameStr);
                if (result == null) return NotFound(new { message = "No image found in Cloudinary folder 'AirtestUpload' to process." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] AIQueryParameters parameters)
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
            return Ok(new { message = "Delete successful" });
        }
    }
}
