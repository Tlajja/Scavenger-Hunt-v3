using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Tasks;

namespace PhotoScavengerHunt.Services
{
    public class TaskService
    {
        private readonly PhotoScavengerHuntDbContext _db;
        private readonly ILogger<TaskService> _logger;

        public TaskService(PhotoScavengerHuntDbContext db, ILogger<TaskService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<HuntTask> CreateTaskAsync(CreateTaskRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.Description))
                    throw new ArgumentException("Task description cannot be empty.");
                if (req.Deadline.HasValue && req.Deadline <= DateTime.UtcNow)
                    throw new ArgumentException("Deadline cannot be in the past.");

                var task = HuntTaskFactory.Create(
                    description: req.Description,
                    authorId: req.AuthorId ?? 0,
                    deadline: req.Deadline,
                    status: HuntTaskStatus.Open);

                _db.Tasks.Add(task);
                await _db.SaveChangesAsync();
                return task;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed while creating task.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task.");
                throw new InvalidOperationException("An unexpected error occurred while creating the task.", ex);
            }
        }

        public async Task<HuntTask> CreateUserTaskAsync(CreateTaskRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.Description))
                    throw new ArgumentException("Task description cannot be empty.");
                if (req.Deadline.HasValue && req.Deadline <= DateTime.UtcNow)
                    throw new ArgumentException("Deadline cannot be in the past.");
                if (!await _db.Users.AnyAsync(u => u.Id == req.AuthorId))
                    throw new ArgumentException("User does not exist.");

                var task = HuntTaskFactory.Create(
                    description: req.Description,
                    authorId: req.AuthorId,
                    deadline: req.Deadline,
                    status: HuntTaskStatus.Open);

                _db.Tasks.Add(task);
                await _db.SaveChangesAsync();
                return task;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed while creating user task.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user task.");
                throw new InvalidOperationException("An unexpected error occurred while creating the user task.", ex);
            }
        }

        public async Task<IEnumerable<HuntTask>> GetTasksAsync()
        {
            try
            {
                return await _db.Tasks.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tasks.");
                throw new InvalidOperationException("An error occurred while fetching tasks.", ex);
            }
        }

        public async Task<HuntTask?> GetTaskByIdAsync(int id)
        {
            try
            {
                return await _db.Tasks.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching task by ID {TaskId}.", id);
                throw new InvalidOperationException("An error occurred while fetching the task.", ex);
            }
        }

        public async Task DeleteUserTaskAsync(int userId, int taskId)
        {
            try
            {
                var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.AuthorId == userId);
                if (task is null)
                    throw new KeyNotFoundException("Task not found or not created by this user.");

                _db.Tasks.Remove(task);
                await _db.SaveChangesAsync();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Delete failed: task not found.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user task.");
                throw new InvalidOperationException("An error occurred while deleting the task.", ex);
            }
        }
    }
}