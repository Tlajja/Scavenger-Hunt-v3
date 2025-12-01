using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Services.Interfaces;
using PhotoScavengerHunt.Repositories;
using PhotoScavengerHunt.Exceptions;

namespace PhotoScavengerHunt.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepo, ILogger<UserService> logger)
        {
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task<(bool Success, string Error, UserProfile? User)> CreateUserAsync(string name, int age)
        {
            try
            {
                await _userRepo.EnsureUsernameIsValidAsync(name);
                
                if (age <= 0 || age > 125)
                    throw new ArgumentException("Invalid age value.");

                var profile = new UserProfile
                {
                    Name = name,
                    Age = age
                };

                await _userRepo.AddAsync(profile);
                await _userRepo.SaveChangesAsync();

                return (true, string.Empty, profile);
            }
            catch (ArgumentException aex)
            {
                return (false, aex.Message, null);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update failed while creating user.");
                return (false, $"Database update failed: {ex.Message}", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating user.");
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Error, List<UserProfile>? Users)> GetUsersAsync()
        {
            try
            {
                var users = await _userRepo.GetAllAsync();
                return (true, string.Empty, users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users.");
                return (false, $"Error retrieving users: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Error, UserProfile? User)> GetUserByIdAsync(int id)
        {
            try
            {
                if(!await _userRepo.ExistsAsync(id))
                throw new ChallengeNotFoundException("User does not exist.");
                var user = await _userRepo.GetByIdAsync(id);
                return (true, string.Empty, user);
            }
            catch (ChallengeNotFoundException)
            {
                return (false, "User not found.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user.");
                return (false, $"Error retrieving user: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Error)> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(id);
                if (user == null)
                    throw new ChallengeNotFoundException("User not found.");
                await _userRepo.RemoveAsync(user);
                await _userRepo.SaveChangesAsync();
                return (true, string.Empty);
            }
            catch (ChallengeNotFoundException)
            {
                return (false, "User not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user.");
                return (false, $"Error deleting user: {ex.Message}");
            }
        }
    }
}