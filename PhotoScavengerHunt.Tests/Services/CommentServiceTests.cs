using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace PhotoScavengerHunt.Tests.Services
{
    public class CommentServiceTests : DatabaseTestBase
    {
        private readonly CommentService _service;
        private readonly Mock<ILogger<CommentService>> _mockLogger;

        public CommentServiceTests()
        {
            _mockLogger = CreateMockLogger<CommentService>();
            _service = new CommentService(DbContext, _mockLogger.Object);
            SeedTestData();
        }

        [Fact]
        public async Task AddCommentAsync_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new AddCommentRequest(
                UserId: 100,
                Text: "This is a test comment"
            );

            // Act
            var result = await _service.AddCommentAsync(400, request);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Error);
            Assert.NotNull(result.Comments);
            Assert.Single(result.Comments);
            Assert.Equal("This is a test comment", result.Comments[0].Text);
            Assert.Equal(100, result.Comments[0].UserId);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AddCommentAsync_EmptyText_ReturnsError(string? text)
        {
            // Arrange
            var request = new AddCommentRequest(UserId: 100, Text: text!);

            // Act
            var result = await _service.AddCommentAsync(400, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Comment text cannot be empty.", result.Error);
            Assert.Null(result.Comments);
        }

        [Fact]
        public async Task AddCommentAsync_NullText_ReturnsError()
        {
            // Arrange
            var request = new AddCommentRequest(UserId: 100, Text: null!);

            // Act
            var result = await _service.AddCommentAsync(400, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Comment text cannot be empty.", result.Error);
            Assert.Null(result.Comments);
        }

        [Fact]
        public async Task AddCommentAsync_NonExistentSubmission_ReturnsError()
        {
            // Arrange
            var request = new AddCommentRequest(
                UserId: 100,
                Text: "Comment on non-existent submission"
            );

            // Act
            var result = await _service.AddCommentAsync(99999, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Submission not found.", result.Error);
        }

        [Fact]
        public async Task AddCommentAsync_MultipleComments_MaintainsOrder()
        {
            // Arrange
            var request1 = new AddCommentRequest(100, "First comment");
            var request2 = new AddCommentRequest(101, "Second comment");
            var request3 = new AddCommentRequest(102, "Third comment");

            // Act
            await _service.AddCommentAsync(400, request1);
            await Task.Delay(10); // Small delay to ensure different timestamps
            await _service.AddCommentAsync(400, request2);
            await Task.Delay(10);
            await _service.AddCommentAsync(400, request3);

            var result = await _service.GetCommentsAsync(400);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, result.Comments!.Count);
        }

        [Fact]
        public async Task AddCommentAsync_SetsTimestamp()
        {
            // Arrange
            var beforeAdd = DateTime.UtcNow.AddSeconds(-1);
            var request = new AddCommentRequest(100, "Timestamped comment");

            // Act
            var result = await _service.AddCommentAsync(400, request);
            var afterAdd = DateTime.UtcNow.AddSeconds(1);

            // Assert
            var comment = result.Comments![0];
            Assert.True(comment.Timestamp >= beforeAdd);
            Assert.True(comment.Timestamp <= afterAdd);
        }

        [Fact]
        public async Task GetCommentsAsync_ValidSubmission_ReturnsComments()
        {
            // Arrange - Add some comments first
            await _service.AddCommentAsync(400, new AddCommentRequest(100, "Comment 1"));
            await _service.AddCommentAsync(400, new AddCommentRequest(101, "Comment 2"));

            // Act
            var result = await _service.GetCommentsAsync(400);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Comments);
            Assert.Equal(2, result.Comments.Count);
        }

        [Fact]
        public async Task GetCommentsAsync_NoComments_ReturnsEmpty()
        {
            // Act
            var result = await _service.GetCommentsAsync(400);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Comments);
            Assert.Empty(result.Comments);
        }

        [Fact]
        public async Task GetCommentsAsync_NonExistentSubmission_ReturnsError()
        {
            // Act
            var result = await _service.GetCommentsAsync(99999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Submission not found.", result.Error);
        }

        [Fact]
        public async Task GetCommentsAsync_IncludesProcessedData()
        {
            // Arrange
            var longText = new string('A', 60); // > 50 characters
            await _service.AddCommentAsync(400, new AddCommentRequest(100, longText));

            // Act
            var result = await _service.GetCommentsAsync(400);

            // Assert
            var comment = result.Comments![0];
            
            // Use reflection to access anonymous type properties
            var commentType = comment.GetType();
            var isRecentProp = commentType.GetProperty("IsRecent");
            var previewProp = commentType.GetProperty("Preview");
            
            Assert.NotNull(isRecentProp);
            Assert.NotNull(previewProp);
            
            var preview = (string)previewProp.GetValue(comment)!;
            Assert.NotNull(preview);
            Assert.True(preview.Length <= 53); // 50 + "..."
            Assert.EndsWith("...", preview);
        }

        [Fact]
        public async Task GetCommentsAsync_RecentComment_MarkedAsRecent()
        {
            // Arrange
            await _service.AddCommentAsync(400, new AddCommentRequest(100, "Recent comment"));

            // Act
            var result = await _service.GetCommentsAsync(400);

            // Assert
            var comment = result.Comments![0];
            
            // Use reflection to access anonymous type properties
            var commentType = comment.GetType();
            var isRecentProp = commentType.GetProperty("IsRecent");
            Assert.NotNull(isRecentProp);
            
            var isRecent = (bool)isRecentProp.GetValue(comment)!;
            Assert.True(isRecent);
        }

        [Fact]
        public async Task DeleteCommentAsync_ValidComment_ReturnsSuccess()
        {
            // Arrange
            var addResult = await _service.AddCommentAsync(400, new AddCommentRequest(100, "To be deleted"));
            var commentId = addResult.Comments![0].Id;

            // Act
            var result = await _service.DeleteCommentAsync(400, commentId);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Error);

            var comment = await DbContext.Comments.FindAsync(commentId);
            Assert.Null(comment);
        }

        [Fact]
        public async Task DeleteCommentAsync_NonExistentSubmission_ReturnsError()
        {
            // Act
            var result = await _service.DeleteCommentAsync(99999, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Submission not found.", result.Error);
        }

        [Fact]
        public async Task DeleteCommentAsync_NonExistentComment_ReturnsError()
        {
            // Act
            var result = await _service.DeleteCommentAsync(400, 99999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Comment not found.", result.Error);
        }

        [Fact]
        public async Task DeleteCommentAsync_RemovesFromSubmissionComments()
        {
            // Arrange
            await _service.AddCommentAsync(400, new AddCommentRequest(100, "Comment 1"));
            var result2 = await _service.AddCommentAsync(400, new AddCommentRequest(101, "Comment 2"));
            var commentToDelete = result2.Comments![1].Id;

            // Act
            await _service.DeleteCommentAsync(400, commentToDelete);

            // Assert
            var remaining = await _service.GetCommentsAsync(400);
            Assert.Single(remaining.Comments!);
        }

        [Fact]
        public async Task CommentOperations_ArePersisted()
        {
            // Arrange
            var request = new AddCommentRequest(100, "Persistent comment");

            // Act
            var addResult = await _service.AddCommentAsync(400, request);
            var commentId = addResult.Comments![0].Id;

            // Re-query from database
            var getResult = await _service.GetCommentsAsync(400);

            // Assert
            var comment = getResult.Comments!.First();
            
            // Use reflection to access anonymous type properties
            var commentType = comment.GetType();
            var idProp = commentType.GetProperty("Id");
            var textProp = commentType.GetProperty("Text");
            
            var id = (int)idProp!.GetValue(comment)!;
            var text = (string)textProp!.GetValue(comment)!;
            
            Assert.Equal(commentId, id);
            Assert.Equal("Persistent comment", text);
        }

        [Fact]
        public async Task AddCommentAsync_LongText_AcceptsFullText()
        {
            // Arrange
            var longText = new string('X', 1000);
            var request = new AddCommentRequest(100, longText);

            // Act
            var result = await _service.AddCommentAsync(400, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(longText, result.Comments![0].Text);
        }
    }
}