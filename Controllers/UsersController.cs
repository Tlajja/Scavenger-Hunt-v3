using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Services.Interfaces;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string name)
        {
            var result = await _userService.CreateUserAsync(name);

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);

            if (!result.Success)
                return NotFound(result.Error);

            return NoContent(); // 204
        }
    }
}