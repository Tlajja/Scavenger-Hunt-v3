using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Users;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly PhotoScavengerHuntDbContext _db;

        public AuthController(PhotoScavengerHuntDbContext db)
        {
            _db = db;
        }

        // Step 1: Register with email and password
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest("Email, password and username are required (cannot be empty).");
            }

            // Validate email format
            if (!IsValidEmail(request.Email))
            {
                return BadRequest("Invalid email format. Email must be in the format: something@something.something");
            }

            if (request.Password.Length < 6)
            {
                return BadRequest("Password must be at least 6 characters long.");
            }

            // Check if email already exists
            if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            {
                return Conflict("Email already registered.");
            }

            // Validate username
            if (!ValidationExtensions.IsValidUsername(request.Username))
            {
                return BadRequest("Username can only contain English letters (a-z, A-Z) and numbers (0-9), with no spaces, and must be between 2 and 20 characters long.");
            }

            // Check if username already exists
            if (await _db.Users.AnyAsync(u => u.Name == request.Username))
            {
                return Conflict("Username already exists. Please choose another.");
            }

            // Validate age
            if (request.Age <= 0 || request.Age > 125)
            {
                return BadRequest("Invalid age value.");
            }

            // Hash the password
            string passwordHash = HashPassword(request.Password);

            // Create user profile
            var userProfile = new UserProfile
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                Name = request.Username,
                Age = request.Age,
                IsRegistered = true
            };

            _db.Users.Add(userProfile);
            await _db.SaveChangesAsync();

            return Ok(new { 
                message = "Registration successful. You can now log in.", 
                userId = userProfile.Id,
                username = userProfile.Name
            });
        }

        // Step 2: Create username after registration
        [HttpPost("create-username")]
        public async Task<IActionResult> CreateUsername(int userId, [FromBody] CreateUsernameRequest request)
        {
            // Find user by ID
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Check if username already created
            if (user.IsRegistered)
            {
                return BadRequest("Username already created for this account.");
            }

            // Validate username
            if (!ValidationExtensions.IsValidUsername(request.Username))
            {
                return BadRequest("Username can only contain English letters (a-z, A-Z) and numbers (0-9), with no spaces, and must be between 2 and 20 characters long.");
            }

            // Check if username already exists
            if (await _db.Users.AnyAsync(u => u.Name == request.Username))
            {
                return Conflict("Username already exists. Please choose another.");
            }

            // Validate age
            if (request.Age <= 0 || request.Age > 125)
            {
                return BadRequest("Invalid age value.");
            }

            // Update user profile
            user.Name = request.Username;
            user.Age = request.Age;
            user.IsRegistered = true;

            await _db.SaveChangesAsync();

            return Ok(new { 
                message = "Username created successfully. You can now log in.", 
                username = user.Name 
            });
        }

        // Step 3: Login with username and password
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required.");
            }

            // Find user by username
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Name == request.Username);
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            // Check if user has completed registration
            if (!user.IsRegistered)
            {
                return BadRequest("Please complete your registration by creating a username.");
            }

            // Verify password
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid username or password.");
            }

            return Ok(new { 
                message = "Login successful.", 
                userId = user.Id,
                username = user.Name
            });
        }

        // Helper method to validate email format
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Regular expression for email validation
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        // Helper method to hash password using SHA256
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Helper method to verify password
        private bool VerifyPassword(string password, string storedHash)
        {
            string hashOfInput = HashPassword(password);
            return hashOfInput == storedHash;
        }
    }
}