using System.Collections.Generic;
using PhotoScavengerHunt.Features.Tasks;

namespace PhotoScavengerHunt.Services.Interfaces;

public interface ITaskService
{
    Task<BasicTask> CreateTaskAsync(CreateTaskRequest req);
    Task<BasicTask> CreateUserTaskAsync(CreateTaskRequest req);
    Task<IEnumerable<BasicTask>> GetTasksAsync();
    Task<BasicTask?> GetTaskByIdAsync(int id);
    Task<BasicTask?> GetRandomTaskForUserAsync(int userId);
    Task DeleteUserTaskAsync(int userId, int taskId);
}