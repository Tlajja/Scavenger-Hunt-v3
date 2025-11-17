using PhotoScavengerHunt.Features.Users;

namespace PhotoScavengerHunt.Repositories;

public interface IUserRepository
{
    Task<bool> ExistsAsync(int id);
    Task<UserProfile?> GetByIdAsync(int id);
    Task EnsureUserExistsAsync(int id, string? errorMessage = null);
    Task IncrementWinsAsync(int userId);
}