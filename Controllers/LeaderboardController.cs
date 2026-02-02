
using Microsoft.EntityFrameworkCore;
using BIL.Service;
using Microsoft.AspNetCore.Mvc;

namespace GameCompetionAnalysisSystem.Controllers
{
    [ApiController]
    [Route("api/leaderboard")]
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
        public async Task<IActionResult> GetTop(int n)
        {
            return Ok(await _service.GetTopAsync(n));
        }
    }


}
