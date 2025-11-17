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

        public async Task EnsureTaskExistsAsync(int id)
        {
            if(!await ExistsAsync(id))
                throw new ChallengeNotFoundException("Task does not exist.");
        }
    }
}