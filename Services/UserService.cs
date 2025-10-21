using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Users;

namespace PhotoScavengerHunt.Services
{
    public class UserService
    {
        private readonly PhotoScavengerHuntDbContext _db;

        public UserService(PhotoScavengerHuntDbContext db)
        {
            _db = db;
        }

        public async Task<(bool Success, string Error, UserProfile? User)> CreateUserAsync(string name, int age)
        {
            try
            {
                // Validation
                if (!ValidationExtensions.IsValidUsername(name))
                    return (false, "Invalid username format. Must be 2–20 alphanumeric characters, no spaces.", null);

                if (await _db.Users.AnyAsync(u => u.Name == name))
                    return (false, "Username already exists.", null);

                if (age <= 0 || age > 125)
                    return (false, "Invalid age value.", null);

                var profile = new UserProfile
                {
                    Name = name,
                    Age = age
                };

                _db.Users.Add(profile);
                await _db.SaveChangesAsync();

                return (true, string.Empty, profile);
            }
            catch (DbUpdateException ex)
            {
                return (false, $"Database update failed: {ex.Message}", null);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Error, List<UserProfile>? Users)> GetUsersAsync()
        {
            try
            {
                var users = await _db.Users.ToListAsync();
                return (true, string.Empty, users);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving users: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Error, UserProfile? User)> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _db.Users.FindAsync(id);
                if (user == null)
                    return (false, "User not found.", null);

                return (true, string.Empty, user);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving user: {ex.Message}", null);
            }
        }
    }
}