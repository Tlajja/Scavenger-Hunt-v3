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
                var challenge = await _challengeService.CreateChallengeAsync(request);
                return CreatedAtAction(nameof(GetChallengeById), new { id = challenge.Id }, challenge);
            }
            catch (ChallengeValidationException ex)
            {
                LogExceptionToFile(ex);
                return BadRequest(new { error = ex.Message });
            }
            catch (ChallengeNotFoundException ex)
            {
                LogExceptionToFile(ex);
                return NotFound(new { error = ex.Message });
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
                var participant = await _challengeService.JoinChallengeAsync(request);
                var joinCode = (request.JoinCode ?? string.Empty).Trim().ToUpperInvariant();
                return Ok(new {participant, joinCode });
            }
            catch (ChallengeValidationException ex)
            {
                LogExceptionToFile(ex);
                return BadRequest(new { error = ex.Message });
            }
            catch (ChallengeNotFoundException ex)
            {
                LogExceptionToFile(ex);
                return NotFound(new { error = ex.Message });
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
                var challenges = await _challengeService.GetChallengesAsync(publicOnly);
                return Ok(challenges);
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
                var challenge = await _challengeService.GetChallengeByIdAsync(id);
                return Ok(challenge);
            }
            catch (ChallengeNotFoundException ex)
            {
                LogExceptionToFile(ex);
                return NotFound(new { error = ex.Message });
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
                await _challengeService.DeleteChallengeAsync(id, userId);
                return NoContent();
            }
            catch (ChallengeValidationException ex)
            {
                LogExceptionToFile(ex);
                return BadRequest(new { error = ex.Message });
            }
            catch (ChallengeNotFoundException ex)
            {
                LogExceptionToFile(ex);
                return NotFound(new { error = ex.Message });
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
                await _challengeService.LeaveChallengeAsync(challengeId, userId);
                return NoContent();
            }
            catch (ChallengeNotFoundException ex)
            {
                LogExceptionToFile(ex);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
                return StatusCode(500, new { error = "An unexpected error occurred while leaving the challenge." });
            }
        }

        [HttpPost("{id}/finalize")]
        public async Task<IActionResult> FinalizeChallenge(int id)
        {
            try
            {
                var challenge = await _challengeService.FinalizeChallengeAsync(id);
                return Ok(challenge);
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
                return StatusCode(500, new { error = ex.Message });
            }
        }
        
        [HttpPost("{id}/advance")]
        public async Task<IActionResult> AdvanceChallenge(int id, [FromQuery] int userId)
        {
            try
            {
                var challenge = await _challengeService.AdvanceChallengeAsync(id, userId);
                return Ok(challenge);
            }
            catch (ChallengeNotFoundException ex)
            {
                LogExceptionToFile(ex);
                return NotFound(new { error = ex.Message });
            }
            catch (ChallengeValidationException ex)
            {
                LogExceptionToFile(ex);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
                return StatusCode(500, new { error = "Unable to advance challenge stage." });
            }
        }
    }
}
