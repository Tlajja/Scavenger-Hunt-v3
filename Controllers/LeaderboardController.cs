using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Features.Leaderboard;
using PhotoScavengerHunt.Services.Interfaces;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;
        private readonly ILogger<LeaderboardController> _logger;

        public LeaderboardController(ILeaderboardService leaderboardService, ILogger<LeaderboardController> logger)
        {
            _leaderboardService = leaderboardService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<LeaderboardEntry>>> GetLeaderboard()
        {
            try
            {
                var leaderboard = await _leaderboardService.GetLeaderboardAsync();
                return Ok(leaderboard);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Handled application error in leaderboard retrieval.");
                return StatusCode(500, new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in GetLeaderboard.");
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving the leaderboard." });
            }
        }

        [HttpGet("halloffame")]
        public async Task<IActionResult> GetHallOfFame([FromQuery] int top = 10)
        {
            try
            {
                var list = await _leaderboardService.GetHallOfFameAsync(top);
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in GetHallOfFame.");
                return StatusCode(500, new { error = "Unable to retrieve hall of fame." });
            }
        }
    }
}