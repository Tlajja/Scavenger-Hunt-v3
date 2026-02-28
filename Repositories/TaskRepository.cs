using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Exceptions;
using PhotoScavengerHunt.Features.Tasks;

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

        public async Task<List<HuntTask>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var set = ids?.ToList() ?? new List<int>();
            return await _dbContext.Tasks.Where(t => set.Contains(t.Id)).ToListAsync();
        }

        public async Task<List<HuntTask>> GetAllAsync()
        {
            return await _dbContext.Tasks.ToListAsync();
        }

        public async Task<HuntTask?> GetRandomForUserAsync(int userId)
        {
            // Only consider tasks that are not expired (no deadline or deadline in the future)
            var query = _dbContext.Tasks
                .Where(t => t.Deadline == null || t.Deadline > DateTime.UtcNow)
                // Do not return tasks authored by the user to keep it fair
                .Where(t => t.AuthorId != userId);

            // Order by NEWID() for SQL Server randomness; fallback to client-side for in-memory
            return await query
                .OrderBy(_ => Guid.NewGuid())
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(HuntTask task)
        {
            await _dbContext.Tasks.AddAsync(task);
        }

        public async Task RemoveAsync(HuntTask task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            // Remove any ChallengeTask references to this task first to avoid FK restriction errors
            var dependents = _dbContext.ChallengeTasks.Where(ct => ct.TaskId == task.Id);
            if (await dependents.AnyAsync())
            {
                _dbContext.ChallengeTasks.RemoveRange(dependents);
            }

            _dbContext.Tasks.Remove(task);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}