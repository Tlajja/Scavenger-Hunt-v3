using Microsoft.EntityFrameworkCore;
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

        public async Task<BasicTask?> GetByIdAsync(int id)
        {
            return await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task EnsureTaskExistsAsync(int id)
        {
            if(!await ExistsAsync(id))
                throw new ChallengeNotFoundException("Task does not exist.");
        }

        public async Task<List<BasicTask>> GetAllAsync()
        {
            return await _dbContext.Tasks.ToListAsync();
        }

        public async Task<List<BasicTask>> GetAllNonExpiredAsync()
        {
            return await _dbContext.Tasks
                .Where(t => t.Deadline == null || t.Deadline > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<BasicTask?> GetRandomAsync(int? excludeAuthorId = null)
        {
            // Only consider tasks that are not expired (no deadline or deadline in the future)
            var query = _dbContext.Tasks
                .Where(t => t.Deadline == null || t.Deadline > DateTime.UtcNow);

            if (excludeAuthorId.HasValue)
            {
                query = query.Where(t => t.AuthorId != excludeAuthorId.Value);
            }

            // Order by NEWID() for SQL Server randomness; fallback to client-side for in-memory
            // EF Core will translate OrderBy(Guid.NewGuid()) to NEWID() on SQL Server
            return await query
                .OrderBy(_ => Guid.NewGuid())
                .FirstOrDefaultAsync();
        }

        public async Task<BasicTask?> GetRandomForUserAsync(int userId)
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

        public async Task AddAsync(BasicTask task)
        {
            await _dbContext.Tasks.AddAsync(task);
        }

        public Task RemoveAsync(BasicTask task)
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