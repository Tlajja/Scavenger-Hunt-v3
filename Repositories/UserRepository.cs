using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Exceptions;
using System.Linq;

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

        public async Task AddAsync(UserProfile user)
        {
            await _dbContext.Users.AddAsync(user);
        }

        public async Task<List<UserProfile>> GetAllAsync()
        {
            return await _dbContext.Users.ToListAsync();
        }

        public Task RemoveAsync(UserProfile user)
        {
            _dbContext.Users.Remove(user);
            return Task.CompletedTask;
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

        public async Task EnsureEmailIsUniqueAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");
            var exists = await _dbContext.Users.AnyAsync(u => u.Email == email);
            if (exists)
                throw new ArgumentException("Email already registered.");
        }

        public async Task<UserProfile?> GetByNameAsync(string username)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Name == username);
        }

        public async Task<Dictionary<int, string>> GetUserNamesAsync(IEnumerable<int> userIds)
        {
            var uniqueUserIds = userIds.Distinct().ToList();
            if (!uniqueUserIds.Any())
                return new Dictionary<int, string>();

            return await _dbContext.Users
                .Where(u => uniqueUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Name);
        }
    }
}