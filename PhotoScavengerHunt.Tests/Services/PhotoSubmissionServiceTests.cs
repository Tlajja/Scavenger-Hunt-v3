using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Repositories;
using PhotoScavengerHunt.Services.Interfaces;
using Moq;
using Xunit;
using System.Text;

namespace PhotoScavengerHunt.Tests.Services
{
    public class PhotoSubmissionServiceTests : DatabaseTestBase
    {
        private readonly PhotoSubmissionService _service;
        private readonly Mock<IStorageService> _mockStorage;

        public PhotoSubmissionServiceTests()
        {
            _mockStorage = new Mock<IStorageService>();
            _mockStorage.Setup(s => s.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
               .ReturnsAsync("https://example.com/uploads/test.jpg");
            _mockStorage.Setup(s => s.DeleteFileAsync(It.IsAny<string>()))
               .Returns(() => Task.CompletedTask);
            
            var photoRepo = new PhotoRepository(DbContext);
            var userRepo = new UserRepository(DbContext);
            var taskRepo = new TaskRepository(DbContext);
            var challengeRepo = new ChallengeRepository(DbContext);
            _service = new PhotoSubmissionService(photoRepo, userRepo, taskRepo, challengeRepo, _mockStorage.Object);

            SeedTestData();
        }

        private IFormFile CreateMockFile(string fileName, string content, string contentType)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            var file = new Mock<IFormFile>();
            
            file.Setup(f => f.FileName).Returns(fileName);
            file.Setup(f => f.Length).Returns(bytes.Length);
            file.Setup(f => f.ContentType).Returns(contentType);
            file.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream target, CancellationToken token) => stream.CopyToAsync(target, token));
            
            return file.Object;
        }

        [Theory]
        [InlineData("test.jpg", "image/jpeg")]
        [InlineData("test.jpeg", "image/jpeg")]
        [InlineData("test.png", "image/png")]
        [InlineData("test.gif", "image/gif")]
        public async Task UploadPhotoAsync_ValidFile_ReturnsSuccess(string fileName, string contentType)
        {
            var file = CreateMockFile(fileName, "fake image content", contentType);

            var result = await _service.UploadPhotoAsync(200, 100, file);

            Assert.True(result.Success);
            Assert.Equal("Photo uploaded successfully.", result.Message);
            Assert.NotNull(result.PhotoUrl);
            Assert.NotNull(result.SubmissionId);
            Assert.StartsWith("https://", result.PhotoUrl);
            Assert.Contains("/uploads/", result.PhotoUrl);
        }

        [Fact]
        public async Task UploadPhotoAsync_NullFile_ReturnsError()
        {
            var result = await _service.UploadPhotoAsync(200, 100, null!);

            Assert.False(result.Success);
            Assert.Equal("No file uploaded.", result.Message);
        }

        [Theory]
        [InlineData("test.txt", "text/plain")]
        [InlineData("test.pdf", "application/pdf")]
        [InlineData("test.exe", "application/octet-stream")]
        public async Task UploadPhotoAsync_InvalidFileType_ReturnsError(string fileName, string contentType)
        {
            var file = CreateMockFile(fileName, "content", contentType);

            var result = await _service.UploadPhotoAsync(200, 100, file);

            Assert.False(result.Success);
            Assert.Contains("Only image files", result.Message);
        }

        [Fact]
        public async Task UploadPhotoAsync_FileTooLarge_ReturnsError()
        {
            var largeContent = new string('x', 10_000_001); // > 10MB
            var bytes = Encoding.UTF8.GetBytes(largeContent);
            var stream = new MemoryStream(bytes);
            var file = new Mock<IFormFile>();
            
            file.Setup(f => f.FileName).Returns("large.jpg");
            file.Setup(f => f.Length).Returns(bytes.Length);
            file.Setup(f => f.ContentType).Returns("image/jpeg");

            var result = await _service.UploadPhotoAsync(200, 100, file.Object);

            Assert.False(result.Success);
            Assert.Contains("File size cannot exceed 10MB", result.Message);
        }

        [Fact]
        public async Task UploadPhotoAsync_NonExistentTask_ReturnsError()
        {
            var file = CreateMockFile("test.jpg", "content", "image/jpeg");

            var result = await _service.UploadPhotoAsync(99999, 100, file);

            Assert.False(result.Success);
            Assert.Equal("Unexpected error: Task does not exist.", result.Message);
        }

        [Fact]
        public async Task UploadPhotoAsync_NonExistentUser_ReturnsError()
        {
            var file = CreateMockFile("test.jpg", "content", "image/jpeg");

            var result = await _service.UploadPhotoAsync(200, 99999, file);

            Assert.False(result.Success);
            Assert.Equal("Unexpected error: User does not exist.", result.Message);
        }

        [Fact]
        public async Task UploadPhotoAsync_CreatesUploadsDirectory()
        {
            var file = CreateMockFile("test.jpg", "content", "image/jpeg");

            await _service.UploadPhotoAsync(200, 100, file);

            _mockStorage.Verify(s => s.UploadFileAsync(It.IsAny<IFormFile>(), "uploads"), Times.Once);
        }

        [Fact]
        public async Task GetSubmissionsForTaskAsync_ReturnsSubmissions()
        {
            var result = await _service.GetSubmissionsForTaskAsync(200);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Seed data has 2 submissions for task 200
            Assert.All(result, s => Assert.Equal(200, s.TaskId));
        }

        [Fact]
        public async Task GetSubmissionsForTaskAsync_NoSubmissions_ReturnsEmpty()
        {
            var result = await _service.GetSubmissionsForTaskAsync(201);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSubmissionsByUserAsync_ReturnsUserSubmissions()
        {
            var result = await _service.GetSubmissionsByUserAsync(100);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.All(result, s => Assert.Equal(100, s.UserId));
        }

        [Fact]
        public async Task GetSubmissionsByUserAsync_NoSubmissions_ReturnsEmpty()
        {
            var result = await _service.GetSubmissionsByUserAsync(102);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task DeleteSubmissionAsync_ValidId_DeletesSubmission()
        {
            var result = await _service.DeleteSubmissionAsync(400);

            Assert.True(result.Success);
            Assert.Equal("Submission deleted successfully.", result.Message);

            var submission = await DbContext.Photos.FindAsync(400);
            Assert.Null(submission);
        }

        [Fact]
        public async Task DeleteSubmissionAsync_InvalidId_ReturnsError()
        {
            var result = await _service.DeleteSubmissionAsync(99999);

            Assert.False(result.Success);
            Assert.Equal("Submission not found.", result.Message);
        }

        [Fact]
        public async Task UploadPhotoAsync_SavesWithCommentsCollection()
        {
            var file = CreateMockFile("test.jpg", "content", "image/jpeg");

            var result = await _service.UploadPhotoAsync(200, 100, file);

            var submission = await DbContext.Photos
                .Include(p => p.Comments)
                .FirstAsync(p => p.Id == result.SubmissionId);
            
            Assert.NotNull(submission.Comments);
            Assert.Empty(submission.Comments);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}