using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Users;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly PhotoScavengerHuntDbContext _db;

        public UsersController(PhotoScavengerHuntDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string name, int age)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Username cannot be empty.\n");
            }
            if (!IsValidUsername(name))
            {
                return BadRequest("Username can only contain English letters and numbers.\n");
            }
            if (await _db.Users.AnyAsync(u => u.Name == name))
            {
                return Conflict("Username already exists.\n");
            }
            if (age <= 0 || age > 125)
            {
                return BadRequest("Incorrect age value.\n");
            }

            var profile = new UserProfile
            {
                Name = name,
                Age = age
            };

            _db.Users.Add(profile);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserById), new { id = profile.Id }, profile);
        }

        [HttpGet]
        public async Task<IEnumerable<UserProfile>> GetUsers() =>
            await _db.Users.ToListAsync();

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _db.Users.FindAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        private bool IsValidUsername(string name)
        {
            string pattern = @"^[a-zA-Z0-9]+$";
            return Regex.IsMatch(name, pattern);
        }
    }
}
