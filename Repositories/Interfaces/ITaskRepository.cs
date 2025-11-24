using PhotoScavengerHunt.Features.Tasks;

namespace PhotoScavengerHunt.Repositories;

public interface ITaskRepository
{
    Task<bool> ExistsAsync(int id);
    Task<BasicTask?> GetByIdAsync(int id);
    Task EnsureTaskExistsAsync(int id);
    Task<List<BasicTask>> GetAllAsync();
    Task<BasicTask?> GetRandomForUserAsync(int userId);
    Task AddAsync(BasicTask task);
    Task RemoveAsync(BasicTask task);
    Task SaveChangesAsync();
}