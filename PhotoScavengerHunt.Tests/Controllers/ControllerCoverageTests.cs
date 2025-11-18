using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PhotoScavengerHunt.Controllers;
using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;
using System.Text;

namespace PhotoScavengerHunt.Tests.Controllers
{
    public class CommentsControllerTests : DatabaseTestBase
    {
        private readonly CommentsController _controller;
        private readonly CommentService _service;

        public CommentsControllerTests()
        {
            var logger = CreateMockLogger<CommentService>();
            _service = new CommentService(DbContext, logger.Object);
            _controller = new CommentsController(_service);
            SeedTestData();
        }

        [Fact]
        public async Task AddComment_ValidRequest_ReturnsOk()
        {
            var request = new AddCommentRequest(100, "Test comment");

            var result = await _controller.AddComment(400, request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var comments = Assert.IsAssignableFrom<List<Comment>>(okResult.Value);
            Assert.NotEmpty(comments);
        }

        [Fact]
        public async Task AddComment_EmptyText_ReturnsBadRequest()
        {
            var request = new AddCommentRequest(100, "");

            var result = await _controller.AddComment(400, request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetCommentsForSubmission_ValidId_ReturnsOk()
        {
            await _service.AddCommentAsync(400, new AddCommentRequest(100, "Test"));

            var result = await _controller.GetCommentsForSubmission(400);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task DeleteComment_ValidId_ReturnsNoContent()
        {
            var addResult = await _service.AddCommentAsync(400, new AddCommentRequest(100, "Test"));
            var commentId = addResult.Comments![0].Id;

            var result = await _controller.DeleteComment(400, commentId);

            Assert.IsType<NoContentResult>(result);
        }
    }

    public class UsersControllerTests : DatabaseTestBase
    {
        private readonly UsersController _controller;
        private readonly UserService _service;

        public UsersControllerTests()
        {
            _service = new UserService(DbContext);
            _controller = new UsersController(_service);
            SeedTestData();
        }

        [Fact]
        public async Task CreateUser_ValidInput_ReturnsCreatedAtAction()
        {
            var result = await _controller.CreateUser("NewUser", 25);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdResult.Value);
        }

        [Fact]
        public async Task CreateUser_InvalidUsername_ReturnsBadRequest()
        {
            var result = await _controller.CreateUser("a", 25);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetUsers_ReturnsOk()
        {
            var result = await _controller.GetUsers();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetUserById_ValidId_ReturnsOk()
        {
            var result = await _controller.GetUserById(100);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetUserById_InvalidId_ReturnsNotFound()
        {
            var result = await _controller.GetUserById(99999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

    }

    public class PhotoSubmissionsControllerTests : DatabaseTestBase
    {
        private readonly PhotoSubmissionsController _controller;
        private readonly PhotoSubmissionService _submissionService;
        private readonly VotesService _votesService;
        private readonly Mock<IWebHostEnvironment> _mockEnv;

        public PhotoSubmissionsControllerTests()
        {
            _mockEnv = new Mock<IWebHostEnvironment>();
            var testPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(testPath);
            _mockEnv.Setup(e => e.WebRootPath).Returns(testPath);

            _submissionService = new PhotoSubmissionService(DbContext, _mockEnv.Object);
            _votesService = new VotesService(DbContext);
            _controller = new PhotoSubmissionsController(_submissionService, _votesService);
            SeedTestData();
        }

        private IFormFile CreateMockFile(string filename, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns(filename);
            file.Setup(f => f.Length).Returns(bytes.Length);
            file.Setup(f => f.ContentType).Returns("image/jpeg");
            file.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream target, CancellationToken token) => stream.CopyToAsync(target, token));
            return file.Object;
        }

        [Fact]
        public async Task UploadPhoto_ValidFile_ReturnsOk()
        {
            var file = CreateMockFile("test.jpg", "fake content");

            var result = await _controller.UploadPhoto(null, 200, 100, file);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UploadPhoto_NullFile_ReturnsBadRequest()
        {
            var result = await _controller.UploadPhoto(null, 200, 100, null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetSubmissionsForTask_ReturnsOk()
        {
            var result = await _controller.GetSubmissionsForTask(200);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetSubmissionsByUser_ReturnsOk()
        {
            var result = await _controller.GetSubmissionsByUser(100);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task VoteOnSubmission_ValidId_ReturnsOk()
        {
            var result = await _controller.VoteOnSubmission(400);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteSubmission_ValidId_ReturnsNoContent()
        {
            var result = await _controller.DeleteSubmission(400);

            Assert.IsType<NoContentResult>(result);
        }
    }
}