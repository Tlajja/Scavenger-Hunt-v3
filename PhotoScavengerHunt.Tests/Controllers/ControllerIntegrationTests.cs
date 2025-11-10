using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoScavengerHunt.Controllers;
using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Features.Tasks;
using PhotoScavengerHunt.Features.Leaderboard;
using PhotoScavengerHunt.Tests.Infrastructure;
using Moq;
using Xunit;

namespace PhotoScavengerHunt.Tests.Controllers
{
    public class AuthenticationControllerTests : DatabaseTestBase
    {
        private readonly AuthenticationController _controller;
        private readonly AuthenticationService _service;

        public AuthenticationControllerTests()
        {
            _service = new AuthenticationService(DbContext);
            _controller = new AuthenticationController(_service);
            SeedTestData();
        }

        [Fact]
        public async Task Register_ValidRequest_ReturnsOk()
        {
            var request = new RegisterRequest(
                Email: "controller@test.com",
                Password: "password123",
                Username: "ControllerUser",
                Age: 25
            );

            var result = await _controller.Register(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            Assert.NotNull(value);
        }

        [Fact]
        public async Task Register_InvalidRequest_ReturnsBadRequest()
        {
            var request = new RegisterRequest("", "pass", "user", 25);

            var result = await _controller.Register(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            // First register
            await _service.RegisterAsync(new RegisterRequest(
                "login@test.com", "password123", "LoginUser", 25));
            
            var request = new LoginRequest("LoginUser", "password123");

            var result = await _controller.Login(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            Assert.NotNull(value);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var request = new LoginRequest("NonExistent", "wrongpass");

            var result = await _controller.Login(request);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }

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
            var request = new CreateTaskRequest(
                Description: "Controller Task",
                Deadline: new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                AuthorId: 100
            );

            var result = await _controller.CreateTask(request);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetTaskById), createdResult.ActionName);
        }

        [Fact]
        public async Task CreateTask_EmptyDescription_ReturnsBadRequest()
        {
            var request = new CreateTaskRequest("", DateTime.UtcNow.AddDays(1), 100);

            var result = await _controller.CreateTask(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetTasks_ReturnsOkWithTasks()
        {
            var result = await _controller.GetTasks();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var tasks = Assert.IsAssignableFrom<IEnumerable<HuntTask>>(okResult.Value);
            Assert.NotEmpty(tasks);
        }

        [Fact]
        public async Task GetTaskById_ValidId_ReturnsOk()
        {
            var result = await _controller.GetTaskById(200);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var task = Assert.IsType<HuntTask>(okResult.Value);
            Assert.Equal(200, task.Id);
        }

        [Fact]
        public async Task GetTaskById_InvalidId_ReturnsNotFound()
        {
            var result = await _controller.GetTaskById(99999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteUserTask_ValidRequest_ReturnsNoContent()
        {
            var result = await _controller.DeleteUserTask(100, 200);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUserTask_InvalidTask_ReturnsNotFound()
        {
            var result = await _controller.DeleteUserTask(100, 99999);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }

    public class VotesControllerTests : DatabaseTestBase
    {
        private readonly VotesController _controller;
        private readonly VotesService _service;

        public VotesControllerTests()
        {
            _service = new VotesService(DbContext);
            _controller = new VotesController(_service);
            SeedTestData();
        }

        [Fact]
        public async Task UpvotePhoto_ValidSubmission_ReturnsOk()
        {
            var result = await _controller.UpvotePhoto(400);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task UpvotePhoto_InvalidSubmission_ReturnsBadRequest()
        {
            var result = await _controller.UpvotePhoto(99999);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }

    public class LeaderboardControllerTests : DatabaseTestBase
    {
        private readonly LeaderboardController _controller;
        private readonly LeaderboardService _service;

        public LeaderboardControllerTests()
        {
            var logger = CreateMockLogger<LeaderboardService>();
            _service = new LeaderboardService(DbContext, logger.Object);
            var controllerLogger = CreateMockLogger<LeaderboardController>();
            _controller = new LeaderboardController(_service, controllerLogger.Object);
            SeedTestData();
        }

        [Fact]
        public async Task GetLeaderboard_ReturnsOkWithEntries()
        {
            var result = await _controller.GetLeaderboard();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var entries = Assert.IsAssignableFrom<List<LeaderboardEntry>>(okResult.Value);
            Assert.NotEmpty(entries);
        }
    }
}