using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Tasks;
using PhotoScavengerHunt.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace PhotoScavengerHunt.Tests.Services
{
    public class TaskServiceTests : DatabaseTestBase
    {
        private readonly TaskService _service;
        private readonly Mock<ILogger<TaskService>> _mockLogger;

        public TaskServiceTests()
        {
            _mockLogger = CreateMockLogger<TaskService>();
            _service = new TaskService(DbContext, _mockLogger.Object);
            SeedTestData();
        }

        [Fact]
        public async Task CreateTaskAsync_ValidRequest_ReturnsTask()
        {
            var request = new CreateTaskRequest(
                Description: "New Task",
                AuthorId: 100
            );

            var result = await _service.CreateTaskAsync(request);

            Assert.NotNull(result);
            Assert.Equal("New Task", result.Description);
            Assert.Equal(100, result.AuthorId);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateTaskAsync_EmptyDescription_ThrowsArgumentException(string? description)
        {
            var request = new CreateTaskRequest(
                Description: description!,
                AuthorId: 100
            );

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateTaskAsync(request));
            Assert.Contains("Task description cannot be empty", exception.Message);
        }

        [Fact]
        public async Task CreateTaskAsync_NullDescription_ThrowsArgumentException()
        {
            var request = new CreateTaskRequest(
                Description: null!,
                AuthorId: 100
            );

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateTaskAsync(request));
            Assert.Contains("Task description cannot be empty", exception.Message);
        }

        [Fact]
        public async Task CreateUserTaskAsync_ValidRequest_ReturnsTask()
        {
            var request = new CreateTaskRequest(
                Description: "User Task",
                AuthorId: 100
            );

            var result = await _service.CreateUserTaskAsync(request);

            Assert.NotNull(result);
            Assert.Equal("User Task", result.Description);
            Assert.Equal(100, result.AuthorId);
        }

        [Fact]
        public async Task CreateUserTaskAsync_NonExistentUser_ThrowsArgumentException()
        {
            var request = new CreateTaskRequest(
                Description: "Task for non-existent user",
                AuthorId: 99999
            );

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateUserTaskAsync(request));
            Assert.Contains("User does not exist", exception.Message);
        }

        [Fact]
        public async Task GetTasksAsync_ReturnsAllTasks()
        {
            var result = await _service.GetTasksAsync();

            Assert.NotNull(result);
            var taskList = result.ToList();
            Assert.True(taskList.Count >= 2); // At least seed data
            Assert.Contains(taskList, t => t.Description == "Test Task 1");
            Assert.Contains(taskList, t => t.Description == "Test Task 2");
        }

        [Fact]
        public async Task GetTaskByIdAsync_ValidId_ReturnsTask()
        {
            var result = await _service.GetTaskByIdAsync(200);

            Assert.NotNull(result);
            Assert.Equal(200, result.Id);
            Assert.Equal("Test Task 1", result.Description);
        }

        [Fact]
        public async Task GetTaskByIdAsync_InvalidId_ReturnsNull()
        {
            var result = await _service.GetTaskByIdAsync(99999);

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteUserTaskAsync_ValidAuthor_DeletesTask()
        {
            // Arrange - Task 200 is authored by user 100
            await _service.DeleteUserTaskAsync(100, 200);

            var task = await DbContext.Tasks.FindAsync(200);
            Assert.Null(task);
        }

        [Fact]
        public async Task DeleteUserTaskAsync_WrongAuthor_ThrowsKeyNotFoundException()
        {
            // Arrange - Task 200 is authored by user 100, not 101
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.DeleteUserTaskAsync(101, 200));
            Assert.Contains("Task not found or not created by this user", exception.Message);
        }

        [Fact]
        public async Task DeleteUserTaskAsync_NonExistentTask_ThrowsKeyNotFoundException()
        {
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.DeleteUserTaskAsync(100, 99999));
            Assert.Contains("Task not found", exception.Message);
        }

        [Fact]
        public async Task TaskCreation_IsPersisted()
        {
            var request = new CreateTaskRequest(
                Description: "Persistent Task",
                AuthorId: 100
            );

            var created = await _service.CreateTaskAsync(request);
            
            var retrieved = await _service.GetTaskByIdAsync(created.Id);

            Assert.NotNull(retrieved);
            Assert.Equal(created.Id, retrieved.Id);
            Assert.Equal("Persistent Task", retrieved.Description);
        }

        [Fact]
        public void HuntTaskFactory_Create_SetsCorrectDefaults()
        {
            var task = HuntTaskFactory.Create(
                description: "Factory Task",
                authorId: 100
            );

            Assert.Equal("Factory Task", task.Description);
            Assert.Equal(100, task.AuthorId);
        }

        [Fact]
        public void HuntTaskFactory_Create_EmptyDescription_ThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => HuntTaskFactory.Create("", 100));
            Assert.Contains("Task description cannot be empty", exception.Message);
        }
    }
}