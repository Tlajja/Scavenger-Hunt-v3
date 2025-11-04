using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotoSubmissionsController : ControllerBase
    {
        private readonly PhotoSubmissionService _submissionService;

        public PhotoSubmissionsController(PhotoSubmissionService submissionService)
        {
            _submissionService = submissionService;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadPhoto([FromForm] int taskId, [FromForm] int userId, IFormFile file)
        {
            var result = await _submissionService.UploadPhotoAsync(taskId, userId, file);

            if (!result.Success)
                return BadRequest(new { error = result.Message });

            return Ok(new
            {
                message = result.Message,
                photoUrl = result.PhotoUrl,
                submissionId = result.SubmissionId
            });
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