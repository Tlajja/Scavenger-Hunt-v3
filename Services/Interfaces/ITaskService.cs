using System.Collections.Generic;
using PhotoScavengerHunt.Features.Tasks;

namespace PhotoScavengerHunt.Services.Interfaces;

public interface ITaskService
{
    Task<HuntTask> CreateTaskAsync(CreateTaskRequest req);
    Task<HuntTask> CreateUserTaskAsync(CreateTaskRequest req);
    Task<IEnumerable<HuntTask>> GetTasksAsync();
    Task<HuntTask?> GetTaskByIdAsync(int id);
    Task<HuntTask?> GetRandomTaskForUserAsync(int userId);
    Task DeleteUserTaskAsync(int userId, int taskId);
}