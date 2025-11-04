using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoScavengerHunt.Controllers;
using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Features.Tasks;
using PhotoScavengerHunt.Features.Hubs;
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
            // Arrange
            var request = new RegisterRequest(
                Email: "controller@test.com",
                Password: "password123",
                Username: "ControllerUser",
                Age: 25
            );

            // Act
            var result = await _controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            Assert.NotNull(value);
        }

        [Fact]
        public async Task Register_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest("", "pass", "user", 25);

            // Act
            var result = await _controller.Register(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            // Arrange - First register
            await _service.RegisterAsync(new RegisterRequest(
                "login@test.com", "password123", "LoginUser", 25));
            
            var request = new LoginRequest("LoginUser", "password123");

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            Assert.NotNull(value);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest("NonExistent", "wrongpass");

            // Act
            var result = await _controller.Login(request);

            // Assert
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

    public class HubControllerTests : DatabaseTestBase
    {
        private readonly HubController _controller;
        private readonly HubService _service;

        public HubControllerTests()
        {
            _service = new HubService(DbContext);
            _controller = new HubController(_service);
            SeedTestData();
        }

        [Fact]
        public async Task CreateHub_ValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var request = new CreateHubRequest("Controller Hub", 100, false);

            // Act
            var result = await _controller.CreateHub(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetHubById), createdResult.ActionName);
        }

        [Fact]
        public async Task CreateHub_EmptyName_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateHubRequest("", 100, false);

            // Act
            var result = await _controller.CreateHub(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task JoinHub_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new JoinHubRequest("TEST01", 102);

            // Act
            var result = await _controller.JoinHub(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task JoinHub_InvalidCode_ReturnsBadRequest()
        {
            // Arrange
            var request = new JoinHubRequest("INVALID", 100);

            // Act
            var result = await _controller.JoinHub(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetHubs_ReturnsOkWithHubs()
        {
            // Act
            var result = await _controller.GetHubs(publicOnly: true);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetHubById_ValidId_ReturnsOk()
        {
            // Act
            var result = await _controller.GetHubById(300);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetHubById_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetHubById(99999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteHub_ValidAdmin_ReturnsNoContent()
        {
            // Act
            var result = await _controller.DeleteHub(300, 100);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteHub_NotAdmin_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.DeleteHub(300, 102);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LeaveHub_ValidMember_ReturnsNoContent()
        {
            // Arrange - Join first
            await _service.JoinHubAsync(new JoinHubRequest("TEST01", 102));

            // Act
            var result = await _controller.LeaveHub(300, 102);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task LeaveHub_NotMember_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.LeaveHub(300, 102);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
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
            // Act
            var result = await _controller.UpvotePhoto(400);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task UpvotePhoto_InvalidSubmission_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.UpvotePhoto(99999);

            // Assert
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
            // Act
            var result = await _controller.GetLeaderboard();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var entries = Assert.IsAssignableFrom<List<LeaderboardEntry>>(okResult.Value);
            Assert.NotEmpty(entries);
        }
    }
}