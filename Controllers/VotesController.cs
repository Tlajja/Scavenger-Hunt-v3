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

        // POST api/votes/{submissionId}?userId=123
        [HttpPost("{submissionId}")]
        public async Task<IActionResult> UpvotePhoto(int submissionId, [FromQuery] int userId)
        {
            var (success, errorMessage, result) = await _votesService.UpvotePhotoAsync(submissionId, userId);

            if (!success)
                return BadRequest(new { error = errorMessage });

            return Ok(result);
        }

        // DELETE api/votes/{submissionId}?userId=123
        [HttpDelete("{submissionId}")]
        public async Task<IActionResult> RemoveVote(int submissionId, [FromQuery] int userId)
        {
            var (success, errorMessage, result) = await _votesService.RemoveVoteAsync(submissionId, userId);

            if (!success)
                return BadRequest(new { error = errorMessage });

            return Ok(result);
        }

        // GET api/votes/task/{taskId}?userId=123
        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetUserVotesForTask(int taskId, [FromQuery] int userId)
        {
            var votes = await _votesService.GetUserVotesForTaskAsync(taskId, userId);
            return Ok(votes);
        }

        // GET api/votes/challenge/{challengeId}?userId=123
        [HttpGet("challenge/{challengeId}")]
        public async Task<IActionResult> GetUserVotesForChallenge(int challengeId, [FromQuery] int userId)
        {
            var votes = await _votesService.GetUserVotesForChallengeAsync(challengeId, userId);
            return Ok(votes);
        }
    }
}