using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Services;

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

        [HttpPost]
        public async Task<IActionResult> CreateChallenge([FromBody] CreateChallengeRequest request)
        {
            var result = await _challengeService.CreateChallengeAsync(request);

            if (!result.Success)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(GetChallengeById), new { id = result.Challenge!.Id }, result.Challenge);
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinChallenge([FromBody] JoinChallengeRequest request)
        {
            var result = await _challengeService.JoinChallengeAsync(request);

            if (!result.Success)
                return BadRequest(new { error = result.Error });

            return Ok(result.Participant);
        }

        [HttpGet]
        public async Task<IActionResult> GetChallenges([FromQuery] bool publicOnly = true)
        {
            var result = await _challengeService.GetChallengesAsync(publicOnly);

            if (!result.Success)
                return StatusCode(500, new { error = result.Error });

            return Ok(result.Challenges);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChallengeById(int id)
        {
            var result = await _challengeService.GetChallengeByIdAsync(id);

            if (!result.Success)
                return NotFound(new { error = result.Error });

            return Ok(result.Challenge);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChallenge(int id, [FromQuery] int userId)
        {
            var result = await _challengeService.DeleteChallengeAsync(id, userId);

            if (!result.Success)
                return BadRequest(new { error = result.Error });

            return NoContent();
        }

        [HttpDelete("{challengeId}/leave")]
        public async Task<IActionResult> LeaveChallenge(int challengeId, [FromQuery] int userId)
        {
            var result = await _challengeService.LeaveChallengeAsync(challengeId, userId);

            if (!result.Success)
                return BadRequest(new { error = result.Error });

            return NoContent();
        }
    }
}
