using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Services.Interfaces;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VotesController : ControllerBase
    {
        private readonly IVotesService _votesService;

        public VotesController(IVotesService votesService)
        {
            _votesService = votesService;
        }

        [HttpPost("{submissionId}")]
        public async Task<IActionResult> UpvotePhoto(int submissionId, [FromQuery] int userId)
        {
            var (success, errorMessage, result) = await _votesService.UpvotePhotoAsync(submissionId, userId);

            if (!success)
                return BadRequest(new { error = errorMessage });

            return Ok(result);
        }
    }
}