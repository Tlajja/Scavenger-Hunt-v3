using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PhotoScavengerHunt.Controllers;
using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using PhotoScavengerHunt.Repositories;
using PhotoScavengerHunt.Services.Interfaces;
using Moq;
using Xunit;
using System.Text;

namespace PhotoScavengerHunt.Tests.Controllers
{
    public class CommentsControllerTests : DatabaseTestBase
    {
        private readonly CommentsController _controller;
        private readonly Mock<ICommentService> _service;
        private readonly Mock<IPhotoRepository> _photoRepo;

        public CommentsControllerTests()
        {
            _service = new Mock<ICommentService>();
            _photoRepo = new Mock<IPhotoRepository>();

            _controller = new CommentsController(_service.Object, _photoRepo.Object);
            SeedTestData();
        }

        [Fact]
        public async Task AddComment_EmptyText_ReturnsBadRequest()
        {
            var submissionId = 400;
            var result = await _controller.AddComment(submissionId, new AddCommentRequest(100, ""));
            Assert.IsType<BadRequestObjectResult>(result);
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

           _submissionService = new PhotoSubmissionService(
                                new PhotoRepository(DbContext),
                                new UserRepository(DbContext),
                                new TaskRepository(DbContext),
                                new ChallengeRepository(DbContext),
                                _mockEnv.Object
                            );
            _votesService = new VotesService(new PhotoRepository(DbContext));
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