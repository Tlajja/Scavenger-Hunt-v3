using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Services.Interfaces;
using PhotoScavengerHunt.Exceptions;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChallengeController : ControllerBase
    {
        private readonly IChallengeService _challengeService;

        public ChallengeController(IChallengeService challengeService)
        {
            _challengeService = challengeService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateChallenge([FromBody] CreateChallengeRequest request)
        {
            var challenge = await _challengeService.CreateChallengeAsync(request);
            return CreatedAtAction(nameof(GetChallengeById), new { id = challenge.Id }, challenge);
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinChallenge([FromBody] JoinChallengeRequest request)
        {
            var participant = await _challengeService.JoinChallengeAsync(request);
            var joinCode = (request.JoinCode ?? string.Empty).Trim().ToUpperInvariant();
            return Ok(new {participant, joinCode });
        }

        [HttpGet]
        public async Task<IActionResult> GetChallenges([FromQuery] bool publicOnly = true)
        {
            var challenges = await _challengeService.GetChallengesAsync(publicOnly);
            return Ok(challenges);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChallengeById(int id)
        {
            var challenge = await _challengeService.GetChallengeByIdAsync(id);
            return Ok(challenge);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChallenge(int id, [FromQuery] int userId)
        {
            await _challengeService.DeleteChallengeAsync(id, userId);
            return NoContent();
        }

        [HttpDelete("{challengeId}/leave")]
        public async Task<IActionResult> LeaveChallenge(int challengeId, [FromQuery] int userId)
        {
            await _challengeService.LeaveChallengeAsync(challengeId, userId);
            return NoContent();
        }

        [HttpPost("{id}/finalize")]
        public async Task<IActionResult> FinalizeChallenge(int id)
        {
            var challenge = await _challengeService.FinalizeChallengeAsync(id);
            return Ok(challenge);
        }

        [HttpPost("{id}/advance")]
        public async Task<IActionResult> AdvanceChallenge(int id, [FromQuery] int userId)
        {
            var challenge = await _challengeService.AdvanceChallengeAsync(id, userId);
            return Ok(challenge);
        }
        
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyChallenges([FromQuery] int userId)
        {
            var list = await _challengeService.GetChallengesForUserAsync(userId);
            return Ok(list);
        }
    }
}
