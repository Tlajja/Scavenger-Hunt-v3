using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Tasks;
using PhotoScavengerHunt.Services.Interfaces;
using PhotoScavengerHunt.Repositories;

namespace PhotoScavengerHunt.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepo;
        private readonly IUserRepository _userRepo;
        private readonly ILogger<TaskService> _logger;

        public TaskService(ITaskRepository taskRepo, IUserRepository userRepo, ILogger<TaskService> logger)
        {
            _taskRepo = taskRepo;
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task<HuntTask> CreateTaskAsync(CreateTaskRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.Description))
                    throw new ArgumentException("Task description cannot be empty.");

                var task = HuntTaskFactory.Create(
                    description: req.Description,
                    authorId: req.AuthorId);

                await _taskRepo.AddAsync(task);
                await _taskRepo.SaveChangesAsync();
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
                if (!await _userRepo.ExistsAsync(req.AuthorId))
                    throw new ArgumentException("User does not exist.");

                var task = HuntTaskFactory.Create(
                    description: req.Description,
                    authorId: req.AuthorId);

                await _taskRepo.AddAsync(task);
                await _taskRepo.SaveChangesAsync();
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
                return await _taskRepo.GetAllAsync();
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
                return await _taskRepo.GetByIdAsync(id);
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
                var task = await _taskRepo.GetByIdAsync(taskId);
                if (task is null || task.AuthorId != userId)
                    throw new KeyNotFoundException("Task not found or not created by this user.");

                await _taskRepo.RemoveAsync(task);
                await _taskRepo.SaveChangesAsync();
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