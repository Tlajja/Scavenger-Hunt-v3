using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Features.Tasks;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private static readonly List<UserProfile> users = new();
        private static int nextUserId = 1;

        [HttpPost]
        public IActionResult CreateUser(string name, int age)
        {
            var profile = new UserProfile
            {
                Id = nextUserId++,
                Name = name,
                Age = age
            };

            users.Add(profile);
            return CreatedAtAction(nameof(GetUserById), new { id = profile.Id }, profile);
        }

        [HttpGet]
        public IEnumerable<UserProfile> GetUsers() => users;

        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            var user = users.FirstOrDefault(u => u.Id == id);
            return user is null ? NotFound() : Ok(user);
        }
    }
}