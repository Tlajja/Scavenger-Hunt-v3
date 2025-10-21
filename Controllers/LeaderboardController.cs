using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Features.Leaderboard;
using PhotoScavengerHunt.Services;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly LeaderboardService _leaderboardService;
        private readonly ILogger<LeaderboardController> _logger;

        public LeaderboardController(LeaderboardService leaderboardService, ILogger<LeaderboardController> logger)
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
    }
}