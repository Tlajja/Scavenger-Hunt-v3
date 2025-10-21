using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Services;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string name, int age)
        {
            var result = await _userService.CreateUserAsync(name, age);

            if (!result.Success)
                return BadRequest(result.Error);

            return CreatedAtAction(nameof(GetUserById), new { id = result.User!.Id }, result.User);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var result = await _userService.GetUsersAsync();

            if (!result.Success)
                return StatusCode(500, result.Error);

            return Ok(result.Users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var result = await _userService.GetUserByIdAsync(id);

            if (!result.Success)
                return NotFound(result.Error);

            return Ok(result.User);
        }
    }
}