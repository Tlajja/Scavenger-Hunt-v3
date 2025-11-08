using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Exceptions;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChallengeController : ControllerBase
    {
        private readonly ChallengeService _challengeService;

        public ChallengeController(ChallengeService challengeService)
        {
            _challengeService = challengeService;
        }

        // Permanent file logger for all exceptions
        private void LogExceptionToFile(Exception ex)
        {
            try
            {
                // Use project root to make Logs folder always visible
                var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                var logPath = Path.Combine(logDirectory, "error_log.txt");
                var message = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {ex.GetType().Name}: {ex.Message}{Environment.NewLine}";
                System.IO.File.AppendAllText(logPath, message);
            }
            catch
            {
                // Fail silently if logging fails
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateChallenge([FromBody] CreateChallengeRequest request)
        {
            try
            {
                var result = await _challengeService.CreateChallengeAsync(request);

                if (!result.Success)
                    return BadRequest(new { error = result.Error });

                return CreatedAtAction(nameof(GetChallengeById), new { id = result.Challenge!.Id }, result.Challenge);
            }
            catch (ChallengeLimitException ex)
            {
                LogExceptionToFile(ex);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
                return StatusCode(500, new { error = "An unexpected error occurred while creating the challenge." });
            }
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinChallenge([FromBody] JoinChallengeRequest request)
        {
            try
            {
                var result = await _challengeService.JoinChallengeAsync(request);

                if (!result.Success)
                    return BadRequest(new { error = result.Error });

                return Ok(result.Participant);
            }
            catch (ChallengeLimitException ex)
            {
                LogExceptionToFile(ex);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
                return StatusCode(500, new { error = "An unexpected error occurred while joining the challenge." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChallenges([FromQuery] bool publicOnly = true)
        {
            try
            {
                var result = await _challengeService.GetChallengesAsync(publicOnly);

                if (!result.Success)
                    return StatusCode(500, new { error = result.Error });

                return Ok(result.Challenges);
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
                return StatusCode(500, new { error = "An unexpected error occurred while fetching challenges." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChallengeById(int id)
        {
            try
            {
                var result = await _challengeService.GetChallengeByIdAsync(id);

                if (!result.Success)
                    return NotFound(new { error = result.Error });

                return Ok(result.Challenge);
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
                return StatusCode(500, new { error = "An unexpected error occurred while fetching challenge details." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChallenge(int id, [FromQuery] int userId)
        {
            try
            {
                var result = await _challengeService.DeleteChallengeAsync(id, userId);

                if (!result.Success)
                    return BadRequest(new { error = result.Error });

                return NoContent();
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
                return StatusCode(500, new { error = "An unexpected error occurred while deleting the challenge." });
            }
        }

        [HttpDelete("{challengeId}/leave")]
        public async Task<IActionResult> LeaveChallenge(int challengeId, [FromQuery] int userId)
        {
            try
            {
                var result = await _challengeService.LeaveChallengeAsync(challengeId, userId);

                if (!result.Success)
                    return BadRequest(new { error = result.Error });

                return NoContent();
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
                return StatusCode(500, new { error = "An unexpected error occurred while leaving the challenge." });
            }
        }
    }
}
