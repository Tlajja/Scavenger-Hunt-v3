using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Services.Interfaces;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotoSubmissionsController : ControllerBase
    {
        private readonly IPhotoSubmissionService _submissionService;
        private readonly IVotesService _votesService;
        
        public PhotoSubmissionsController(IPhotoSubmissionService submissionService, IVotesService votesService)
        {
            _submissionService = submissionService;
            _votesService = votesService;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadPhoto([FromForm] int? challengeId, [FromForm] int? taskId, [FromForm] int userId, IFormFile file)
        {
            try
            {
                var result = await _submissionService.UploadPhotoAsync(taskId, userId, file, challengeId);
                if (!result.Success)
                    return BadRequest(new { error = result.Message });

                return Ok(new
                {
                    message = result.Message,
                    photoUrl = result.PhotoUrl,
                    submissionId = result.SubmissionId
                });
            }
            catch (Exception ex)
            {
                // keep your LogExceptionToFile helper if present
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetSubmissionsForTask(int taskId)
        {
            try
            {
                var submissions = await _submissionService.GetSubmissionsForTaskAsync(taskId);
                return Ok(submissions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetSubmissionsByUser(int userId)
        {
            try
            {
                var submissions = await _submissionService.GetSubmissionsByUserAsync(userId);
                return Ok(submissions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSubmissionsByChallenge([FromQuery] int? challengeId, [FromQuery] int? taskId)
        {
            try
            {
                if (taskId.HasValue)
                {
                    var subs = await _submissionService.GetSubmissionsForTaskAsync(taskId.Value);
                    return Ok(subs);
                }
                if (challengeId.HasValue)
                {
                    var subs = await _submissionService.GetSubmissionsForChallengeAsync(challengeId.Value);
                    return Ok(subs);
                }
                return BadRequest(new { error = "Provide either challengeId or taskId as query parameter." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("{id}/vote")]
        public async Task<IActionResult> VoteOnSubmission(int id, [FromQuery] int userId)
        {
            var (success, errorMessage, result) = await _votesService.UpvotePhotoAsync(id, userId);
            if (!success)
            {
                if (errorMessage?.Contains("not") ?? false)
                    return NotFound(new { error = errorMessage });
                return BadRequest(new { error = errorMessage });
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubmission(int id)
        {
            var result = await _submissionService.DeleteSubmissionAsync(id);

            if (!result.Success)
                return NotFound(new { error = result.Message });

            return NoContent(); // HTTP 204 — sėkmingas ištrynimas be turinio
        }
    }
}