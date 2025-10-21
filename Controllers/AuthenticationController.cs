using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Services;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationService _authService;

        public AuthenticationController(AuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            if (!result.Success)
                return BadRequest(result.Message);

            dynamic? d = result.Data;
            return Ok(new { message = result.Message, userId = d?.userId, username = d?.username });
        }

        [HttpPost("create-username")]
        public async Task<IActionResult> CreateUsername(int userId, [FromBody] CreateUsernameRequest request)
        {
            var result = await _authService.CreateUsernameAsync(userId, request);
            if (!result.Success)
                return BadRequest(result.Message);

            dynamic? d = result.Data;
            // CreateUsername returns username; include userId for consistency if available
            return Ok(new { message = result.Message, userId = userId, username = d?.username });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (!result.Success)
                return Unauthorized(result.Message);

            dynamic? d = result.Data;
            return Ok(new { message = result.Message, userId = d?.userId, username = d?.username });
        }
    }
}