using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Tasks;
using PhotoScavengerHunt.Exceptions;

namespace PhotoScavengerHunt.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly PhotoScavengerHuntDbContext _dbContext;
        public TaskRepository(PhotoScavengerHuntDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbContext.Tasks.AnyAsync(t => t.Id == id);
        }

        public async Task<HuntTask?> GetByIdAsync(int id)
        {
            return await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<HuntTask>> GetAllAsync()
        {
            return await _dbContext.Tasks.ToListAsync();
        }

        public async Task AddAsync(HuntTask task)
        {
            await _dbContext.Tasks.AddAsync(task);
        }

        public Task RemoveAsync(HuntTask task)
        {
            _dbContext.Tasks.Remove(task);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}