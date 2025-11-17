using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PhotoScavengerHunt.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly PhotoScavengerHuntDbContext dbContext;

        public AuthenticationService(PhotoScavengerHuntDbContext db)
        {
            dbContext = db;
        }

        public async Task<(bool Success, string Message, object? Data)> RegisterAsync(RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Username))
                    return (false, "Email, password, and username are required.", null);

                if (!IsValidEmail(request.Email))
                    return (false, "Invalid email format.", null);

                if (request.Password.Length < 6)
                    return (false, "Password must be at least 6 characters long.", null);

                if (await dbContext.Users.AnyAsync(u => u.Email == request.Email))
                    return (false, "Email already registered.", null);

                if (!ValidationExtensions.IsValidUsername(request.Username))
                    return (false, "Invalid username format.", null);

                if (await dbContext.Users.AnyAsync(u => u.Name == request.Username))
                    return (false, "Username already exists.", null);

                if (request.Age <= 0 || request.Age > 125)
                    return (false, "Invalid age value.", null);

                string passwordHash = HashPassword(request.Password);

                var userProfile = new UserProfile
                {
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    Name = request.Username,
                    Age = request.Age,
                    IsRegistered = true
                };

                dbContext.Users.Add(userProfile);
                await dbContext.SaveChangesAsync();

                return (true, "Registration successful.", new
                {
                    userId = userProfile.Id,
                    username = userProfile.Name
                });
            }
            catch (Exception ex)
            {
                return (false, $"Error during registration: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, object? Data)> LoginAsync(LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                    return (false, "Username and password are required.", null);

                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Name == request.Username);
                if (user == null)
                    return (false, "Invalid username or password.", null);

                if (!user.IsRegistered)
                    return (false, "Please complete registration first.", null);

                if (!VerifyPassword(request.Password, user.PasswordHash))
                    return (false, "Invalid username or password.", null);

                return (true, "Login successful.", new
                {
                    userId = user.Id,
                    username = user.Name
                });
            }
            catch (Exception ex)
            {
                return (false, $"Error during login: {ex.Message}", null);
            }
        }


        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }
    }
}