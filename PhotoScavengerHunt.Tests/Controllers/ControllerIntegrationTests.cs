using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoScavengerHunt.Controllers;
using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Features.Tasks;
using PhotoScavengerHunt.Tests.Infrastructure;
using Moq;
using Xunit;

namespace PhotoScavengerHunt.Tests.Controllers
{
    public class TasksControllerTests : DatabaseTestBase
    {
        private readonly TasksController _controller;
        private readonly TaskService _service;

        public TasksControllerTests()
        {
            var logger = CreateMockLogger<TaskService>();
            _service = new TaskService(DbContext, logger.Object);
            var controllerLogger = CreateMockLogger<TasksController>();
            _controller = new TasksController(_service, controllerLogger.Object);
            SeedTestData();
        }

        [Fact]
        public async Task CreateTask_ValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var request = new CreateTaskRequest(
                Description: "Controller Task",
                Deadline: new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                AuthorId: 100
            );

            // Act
            var result = await _controller.CreateTask(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetTaskById), createdResult.ActionName);
        }

        [Fact]
        public async Task CreateTask_EmptyDescription_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateTaskRequest("", DateTime.UtcNow.AddDays(1), 100);

            // Act
            var result = await _controller.CreateTask(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetTasks_ReturnsOkWithTasks()
        {
            // Act
            var result = await _controller.GetTasks();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var tasks = Assert.IsAssignableFrom<IEnumerable<HuntTask>>(okResult.Value);
            Assert.NotEmpty(tasks);
        }

        [Fact]
        public async Task GetTaskById_ValidId_ReturnsOk()
        {
            // Act
            var result = await _controller.GetTaskById(200);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var task = Assert.IsType<HuntTask>(okResult.Value);
            Assert.Equal(200, task.Id);
        }

        [Fact]
        public async Task GetTaskById_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetTaskById(99999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteUserTask_ValidRequest_ReturnsNoContent()
        {
            // Act
            var result = await _controller.DeleteUserTask(100, 200);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUserTask_InvalidTask_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteUserTask(100, 99999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
    
}