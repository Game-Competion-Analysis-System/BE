
using Microsoft.EntityFrameworkCore;
using BIL.Service;
using DAL.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameCompetionAnalysisSystem.Controllers
{
    [ApiController]
    [Route("api/leaderboard")]
    [Authorize]
    public class LeaderboardController(ILeaderboardService service) : ControllerBase
    {
        [HttpPost("from-ocr/{analysisId}")]
        public async Task<IActionResult> ParseOcr(int analysisId)
        {
            await service.ProcessOcrAsync(analysisId);
            return Ok("Parsed & saved leaderboard");
        }

        [HttpGet("top/{n}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTop(int n)
        {
            return Ok(await service.GetTopAsync(n));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] QueryParameters parameters)
        {
            return Ok(await service.GetAllAsync(parameters));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var lb = await service.GetByIdAsync(id);
            if (lb == null) return NotFound();
            return Ok(lb);
        }

        [HttpGet("{id}/entries")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEntries(int id)
        {
            return Ok(await service.GetEntriesByLeaderboardIdAsync(id));
        }

        [HttpGet("{id}/sorted")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSortedEntries(int id)
        {
            return Ok(await service.GetSortedEntriesByLeaderboardIdAsync(id));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await service.DeleteAsync(id);
            return Ok(new { message = "Leaderboard deleted successfully" });
        }
    }


}
