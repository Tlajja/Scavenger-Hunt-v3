using PhotoScavengerHunt.Features.Users;

namespace PhotoScavengerHunt.Repositories;

public interface IUserRepository
{
    Task<bool> ExistsAsync(int id);
    Task<UserProfile?> GetByIdAsync(int id);
    Task EnsureUserExistsAsync(int id, string? errorMessage = null);
    Task IncrementWinsAsync(int userId);
    Task<bool> ExistsByNameAsync(string name);
    Task AddAsync(UserProfile user);
    Task<List<UserProfile>> GetAllAsync();
    Task RemoveAsync(UserProfile user);
    Task SaveChangesAsync();
    Task EnsureUsernameIsValidAsync(string name);
}