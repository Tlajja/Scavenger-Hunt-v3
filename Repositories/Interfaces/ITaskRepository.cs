using PhotoScavengerHunt.Features.Tasks;

namespace PhotoScavengerHunt.Repositories;

public interface ITaskRepository
{
    Task<bool> ExistsAsync(int id);
    Task<HuntTask?> GetByIdAsync(int id);
    Task<List<HuntTask>> GetByIdsAsync(IEnumerable<int> ids);
    Task<List<HuntTask>> GetAllAsync();
    Task<HuntTask?> GetRandomForUserAsync(int userId);
    Task AddAsync(HuntTask task);
    Task RemoveAsync(HuntTask task);
    Task SaveChangesAsync();
}