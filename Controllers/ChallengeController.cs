using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Services.Interfaces;

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
            if ((request.TaskIds == null || !request.TaskIds.Any()) && Request.ContentType?.Contains("application/json") == true)
            {
                try
                {
                    using var sr = new StreamReader(Request.Body);
                    var body = await sr.ReadToEndAsync();
                    if (!string.IsNullOrWhiteSpace(body) && body.Contains("\"TaskId\""))
                    {
                        var doc = System.Text.Json.JsonDocument.Parse(body);
                        if (doc.RootElement.TryGetProperty("TaskId", out var tidProp) && tidProp.TryGetInt32(out var singleId))
                        {
                            request = new CreateChallengeRequest(request.Name, request.CreatorId, new[] { singleId }, request.Deadline, request.IsPrivate, request.MaxParticipants);
                        }
                    }
                }
                catch {}
            }
            var challenge = await _challengeService.CreateChallengeAsync(request);
            return CreatedAtAction(nameof(GetChallengeById), new { id = challenge.Id }, challenge);
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinChallenge([FromBody] JoinChallengeRequest request)
        {
            var participant = await _challengeService.JoinChallengeAsync(request);
            var joinCode = (request.JoinCode ?? string.Empty).Trim().ToUpperInvariant();
            return Ok(new { participant, joinCode });
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

        [HttpGet("map")]
        public async Task<IActionResult> GetChallengesForMap()
        {
            var challenges = await _challengeService.GetChallengesAsync(publicOnly: true);
            
            var mapChallenges = challenges
                .Where(c => c.Status == ChallengeStatus.Open &&
                           c.Latitude.HasValue &&
                           c.Longitude.HasValue)
                .Select(c => new ChallengeMapDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    JoinCode = c.JoinCode,
                    ParticipantCount = c.Participants?.Count ?? 0,
                    MaxParticipants = c.MaxParticipants ?? 10,
                    Location = new Location
                    {
                        Latitude = c.Latitude!.Value,
                        Longitude = c.Longitude!.Value,
                        LocationName = c.LocationName ?? ""
                    }
                })
                .ToList();

            return Ok(mapChallenges);
        }

        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearbyChallenges(
            [FromQuery] double lat,
            [FromQuery] double lng,
            [FromQuery] double radiusKm = 10)
        {
            var challenges = await _challengeService.GetChallengesAsync(publicOnly: true);
            
            var nearbyChallenges = challenges
                .Where(c => c.Status == ChallengeStatus.Open &&
                           c.Latitude.HasValue &&
                           c.Longitude.HasValue)
                .Select(c => new
                {
                    Challenge = c,
                    Distance = PhotoScavengerHunt.Services.GeoCalculator.CalculateDistanceKm(
                        lat, lng, c.Latitude!.Value, c.Longitude!.Value)
                })
                .Where(x => x.Distance <= radiusKm)
                .OrderBy(x => x.Distance)
                .Select(x => new ChallengeMapDto
                {
                    Id = x.Challenge.Id,
                    Name = x.Challenge.Name,
                    JoinCode = x.Challenge.JoinCode,
                    ParticipantCount = x.Challenge.Participants?.Count ?? 0,
                    MaxParticipants = x.Challenge.MaxParticipants ?? 10,
                    Location = new Location
                    {
                        Latitude = x.Challenge.Latitude!.Value,
                        Longitude = x.Challenge.Longitude!.Value,
                        LocationName = x.Challenge.LocationName ?? ""
                    }
                })
                .ToList();

            return Ok(nearbyChallenges);
        }
    }
}