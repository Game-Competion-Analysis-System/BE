
using Microsoft.EntityFrameworkCore;
using BIL.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameCompetionAnalysisSystem.Controllers
{
    [ApiController]
    [Route("api/leaderboard")]
    [Authorize]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _service;

        public LeaderboardController(ILeaderboardService service)
        {
            _service = service;
        }

        [HttpPost("from-ocr/{analysisId}")]
        public async Task<IActionResult> ParseOcr(int analysisId)
        {
            await _service.ProcessOcrAsync(analysisId);
            return Ok("Parsed & saved leaderboard");
        }

        [HttpGet("top/{n}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTop(int n)
        {
            return Ok(await _service.GetTopAsync(n));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var lb = await _service.GetByIdAsync(id);
            if (lb == null) return NotFound();
            return Ok(lb);
        }

        [HttpGet("{id}/entries")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEntries(int id)
        {
            return Ok(await _service.GetEntriesByLeaderboardIdAsync(id));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok(new { message = "Leaderboard deleted successfully" });
        }
    }


}
