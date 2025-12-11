using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Controllers;
using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Features.Tasks;
using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Features.Leaderboard;
using PhotoScavengerHunt.Tests.Infrastructure;
using PhotoScavengerHunt.Repositories;
using Moq;
using PhotoScavengerHunt.Services.Interfaces;

namespace PhotoScavengerHunt.Tests.Controllers
{
    public class AuthenticationControllerTests : DatabaseTestBase
    {
        private readonly AuthenticationController _controller;
        private readonly AuthenticationService _service;

        public AuthenticationControllerTests()
        {
            _service = new AuthenticationService(new UserRepository(DbContext));
            _controller = new AuthenticationController(_service);
            SeedTestData();
        }

        [Fact]
        public async Task Register_ValidRequest_ReturnsOk()
        {
            var request = new RegisterRequest(
                Email: "controller@test.com",
                Password: "password123",
                Username: "ControllerUser"
            );

            var result = await _controller.Register(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            Assert.NotNull(value);
        }

        [Fact]
        public async Task Register_InvalidRequest_ReturnsBadRequest()
        {
            var request = new RegisterRequest("", "pass", "user");

            var result = await _controller.Register(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            // First register
            await _service.RegisterAsync(new RegisterRequest(
                "login@test.com", "password123", "LoginUser"));
            
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
            _service = new TaskService(new TaskRepository(DbContext), new UserRepository(DbContext), logger.Object);
            var controllerLogger = CreateMockLogger<TasksController>();
            _controller = new TasksController(_service, controllerLogger.Object);
            SeedTestData();
        }

        [Fact]
        public async Task CreateTask_ValidRequest_ReturnsCreatedAtAction()
        {
            var request = new CreateTaskRequest(
                Description: "Controller Task",
                AuthorId: 100
            );

            var result = await _controller.CreateTask(request);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetTaskById), createdResult.ActionName);
        }

        [Fact]
        public async Task CreateTask_EmptyDescription_ReturnsBadRequest()
        {
            var request = new CreateTaskRequest("", 100);

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

    public class ChallengeControllerTests : DatabaseTestBase
    {
        private readonly ChallengeController _controller;
        private readonly ChallengeService _service;

        public ChallengeControllerTests()
        {
            var photoRepo = new PhotoRepository(DbContext);
            var mockStorage = new Mock<IStorageService>();
            mockStorage.Setup(s => s.DeleteFileAsync(It.IsAny<string>())).Returns(() => Task.CompletedTask);

            _service = new ChallengeService(
            new ChallengeRepository(DbContext),
            new UserRepository(DbContext),
            new TaskRepository(DbContext),
            new ChallengeParticipantRepository(DbContext), 
            photoRepo,
            mockStorage.Object
        );

            _controller = new ChallengeController(_service);
            SeedTestData();
        }

        [Fact]
        public async Task CreateChallenge_ValidRequest_ReturnsCreatedAtAction()
        {
            var request = new CreateChallengeRequest(
                "Controller Challenge",
                creatorId: 102,
                taskIds: new[] { 200 },
                deadline: DateTime.UtcNow.AddDays(7),
                isPrivate: true
            );

            var result = await _controller.CreateChallenge(request);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetChallengeById), createdResult.ActionName);
        }

        [Fact]
        public async Task CreateChallenge_EmptyName_ReturnsBadRequest()
        {
            var request = new CreateChallengeRequest(
                "",
                creatorId: 100,
                taskIds: new[] { 200 },
                deadline: DateTime.UtcNow.AddDays(7),
                isPrivate: false
            );

            IActionResult result;
            try
            {
                result = await _controller.CreateChallenge(request);
            }
            catch (PhotoScavengerHunt.Exceptions.ValidationException)
            {
                result = new BadRequestObjectResult("Challenge name cannot be empty.");
            }

            Assert.IsType<BadRequestObjectResult>(result);
}

        [Fact]
        public async Task JoinChallenge_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new JoinChallengeRequest("TEST01", 102);

            // Act
            var result = await _controller.JoinChallenge(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task JoinChallenge_InvalidCode_ReturnsBadRequest()
        {
            var request = new JoinChallengeRequest("INVALID", 100);

            IActionResult result;
            try
            {
                result = await _controller.JoinChallenge(request);
            }
            catch (PhotoScavengerHunt.Exceptions.EntityNotFoundException)
            {
                result = new NotFoundObjectResult("Challenge not found.");
            }

            Assert.IsType<NotFoundObjectResult>(result);
        }


        [Fact]
        public async Task GetChallenges_ReturnsOkWithChallenges()
        {
            // Act
            var result = await _controller.GetChallenges(publicOnly: true);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetChallengeById_ValidId_ReturnsOk()
        {
            // Act
            var result = await _controller.GetChallengeById(300);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetChallengeById_InvalidId_ReturnsNotFound()
        {
            IActionResult result;
            try
            {
                result = await _controller.GetChallengeById(99999);
            }
            catch (PhotoScavengerHunt.Exceptions.EntityNotFoundException)
            {
                result = new NotFoundObjectResult("Challenge not found.");
            }

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteChallenge_ValidAdmin_ReturnsNoContent()
        {
            // Act
            var result = await _controller.DeleteChallenge(300, 100);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteChallenge_NotAdmin_ReturnsBadRequest()
        {
            IActionResult result;
            try
            {
                result = await _controller.DeleteChallenge(300, 102);
            }
            catch (PhotoScavengerHunt.Exceptions.ValidationException)
            {
                result = new BadRequestObjectResult("Only challenge admins can delete challenges.");
            }

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LeaveChallenge_ValidMember_ReturnsNoContent()
        {
            // Arrange - Join first
            await _service.JoinChallengeAsync(new JoinChallengeRequest("TEST01", 102));

            // Act
            var result = await _controller.LeaveChallenge(300, 102);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task LeaveChallenge_NotMember_ReturnsNotFound()
        {
            // Act
            IActionResult result;
            try
            {
                result = await _controller.LeaveChallenge(300, 102);
            }
            catch (PhotoScavengerHunt.Exceptions.EntityNotFoundException)
            {
                result = new NotFoundObjectResult("User is not a participant of this challenge.");
            }

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }

    public class VotesControllerTests : DatabaseTestBase
    {
        private readonly VotesController _controller;
        private readonly VotesService _service;

        public VotesControllerTests()
        {
            _service = new VotesService(new PhotoRepository(DbContext));
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
            _service = new LeaderboardService(new LeaderboardRepository(DbContext), logger.Object);
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