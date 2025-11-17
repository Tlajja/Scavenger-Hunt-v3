using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Exceptions;

namespace PhotoScavengerHunt.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly PhotoScavengerHuntDbContext _dbContext;

        public UserRepository(PhotoScavengerHuntDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserProfile?> GetByIdAsync(int id)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbContext.Users.AnyAsync(u => u.Id == id);
        }

        public async Task EnsureUserExistsAsync(int id, string? errorMessage = null)
        {
            if(!await ExistsAsync(id))
                throw new ChallengeNotFoundException(errorMessage ?? "User does not exist.");
        }

        public async Task IncrementWinsAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                throw new ChallengeNotFoundException("User does not exist.");

            user.Wins += 1;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _dbContext.Users.AnyAsync(u => u.Name == name);
        }

        public async Task AddAsync(UserProfile user)
        {
            await _dbContext.Users.AddAsync(user);
        }

        public async Task<List<UserProfile>> GetAllAsync()
        {
            return await _dbContext.Users.ToListAsync();
        }

        public async Task RemoveAsync(UserProfile user)
        {
            _dbContext.Users.Remove(user);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task EnsureUsernameIsValidAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || !ValidationExtensions.IsValidUsername(name))
                throw new ArgumentException("Invalid username format");

            var exists = await _dbContext.Users.AnyAsync(u => u.Name == name);
            if (exists)
                throw new ArgumentException("Username already exists.");
        }
    }
}