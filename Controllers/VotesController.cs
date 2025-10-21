using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Services;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VotesController : ControllerBase
    {
        private readonly VotesService _votesService;

        public VotesController(VotesService votesService)
        {
            _votesService = votesService;
        }

        [HttpPost("{submissionId}")]
        public async Task<IActionResult> UpvotePhoto(int submissionId)
        {
            var (success, errorMessage, result) = await _votesService.UpvotePhotoAsync(submissionId);

            if (!success)
                return BadRequest(new { error = errorMessage });

            return Ok(result);
        }
    }
}