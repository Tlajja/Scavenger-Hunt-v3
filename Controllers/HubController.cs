using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Features.Hubs;
using PhotoScavengerHunt.Services;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HubController : ControllerBase
    {
        private readonly HubService _hubService;

        public HubController(HubService hubService)
        {
            _hubService = hubService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateHub([FromBody] CreateHubRequest request)
        {
            var result = await _hubService.CreateHubAsync(request);

            if (!result.Success)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(GetHubById), new { id = result.Hub!.Id }, result.Hub);
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinHub([FromBody] JoinHubRequest request)
        {
            var result = await _hubService.JoinHubAsync(request);

            if (!result.Success)
                return BadRequest(new { error = result.Error });

            return Ok(result.Member);
        }

        [HttpGet]
        public async Task<IActionResult> GetHubs([FromQuery] bool publicOnly = true)
        {
            var result = await _hubService.GetHubsAsync(publicOnly);

            if (!result.Success)
                return StatusCode(500, new { error = result.Error });

            return Ok(result.Hubs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHubById(int id)
        {
            var result = await _hubService.GetHubByIdAsync(id);

            if (!result.Success)
                return NotFound(new { error = result.Error });

            return Ok(result.Hub);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHub(int id, [FromQuery] int userId)
        {
            var result = await _hubService.DeleteHubAsync(id, userId);

            if (!result.Success)
                return BadRequest(new { error = result.Error });

            return NoContent(); // 204
        }

        [HttpDelete("{hubId}/leave")]
        public async Task<IActionResult> LeaveHub(int hubId, [FromQuery] int userId)
        {
            var result = await _hubService.LeaveHubAsync(hubId, userId);

            if (!result.Success)
                return BadRequest(new { error = result.Error });

            return NoContent(); // 204
        }
    }
}

