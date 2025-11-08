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
            var request = new AddCommentRequest(
                UserId: 100,
                Text: "This is a test comment"
            );

            var result = await _service.AddCommentAsync(400, request);

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
            var request = new AddCommentRequest(UserId: 100, Text: text!);

            var result = await _service.AddCommentAsync(400, request);

            Assert.False(result.Success);
            Assert.Equal("Comment text cannot be empty.", result.Error);
            Assert.Null(result.Comments);
        }

        [Fact]
        public async Task AddCommentAsync_NullText_ReturnsError()
        {
            var request = new AddCommentRequest(UserId: 100, Text: null!);

            var result = await _service.AddCommentAsync(400, request);

            Assert.False(result.Success);
            Assert.Equal("Comment text cannot be empty.", result.Error);
            Assert.Null(result.Comments);
        }

        [Fact]
        public async Task AddCommentAsync_NonExistentSubmission_ReturnsError()
        {
            var request = new AddCommentRequest(
                UserId: 100,
                Text: "Comment on non-existent submission"
            );

            var result = await _service.AddCommentAsync(99999, request);

            Assert.False(result.Success);
            Assert.Equal("Submission not found.", result.Error);
        }

        [Fact]
        public async Task AddCommentAsync_MultipleComments_MaintainsOrder()
        {
            var request1 = new AddCommentRequest(100, "First comment");
            var request2 = new AddCommentRequest(101, "Second comment");
            var request3 = new AddCommentRequest(102, "Third comment");

            await _service.AddCommentAsync(400, request1);
            await Task.Delay(10); // Small delay to ensure different timestamps
            await _service.AddCommentAsync(400, request2);
            await Task.Delay(10);
            await _service.AddCommentAsync(400, request3);

            var result = await _service.GetCommentsAsync(400);

            Assert.True(result.Success);
            Assert.Equal(3, result.Comments!.Count);
        }

        [Fact]
        public async Task AddCommentAsync_SetsTimestamp()
        {
            var beforeAdd = DateTime.UtcNow.AddSeconds(-1);
            var request = new AddCommentRequest(100, "Timestamped comment");

            var result = await _service.AddCommentAsync(400, request);
            var afterAdd = DateTime.UtcNow.AddSeconds(1);

            var comment = result.Comments![0];
            Assert.True(comment.Timestamp >= beforeAdd);
            Assert.True(comment.Timestamp <= afterAdd);
        }

        [Fact]
        public async Task GetCommentsAsync_ValidSubmission_ReturnsComments()
        {
            await _service.AddCommentAsync(400, new AddCommentRequest(100, "Comment 1"));
            await _service.AddCommentAsync(400, new AddCommentRequest(101, "Comment 2"));

            var result = await _service.GetCommentsAsync(400);

            Assert.True(result.Success);
            Assert.NotNull(result.Comments);
            Assert.Equal(2, result.Comments.Count);
        }

        [Fact]
        public async Task GetCommentsAsync_NoComments_ReturnsEmpty()
        {
            var result = await _service.GetCommentsAsync(400);

            Assert.True(result.Success);
            Assert.NotNull(result.Comments);
            Assert.Empty(result.Comments);
        }

        [Fact]
        public async Task GetCommentsAsync_NonExistentSubmission_ReturnsError()
        {
            var result = await _service.GetCommentsAsync(99999);

            Assert.False(result.Success);
            Assert.Equal("Submission not found.", result.Error);
        }

        [Fact]
        public async Task GetCommentsAsync_IncludesProcessedData()
        {
            var longText = new string('A', 60); // > 50 characters
            await _service.AddCommentAsync(400, new AddCommentRequest(100, longText));

            var result = await _service.GetCommentsAsync(400);

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
            await _service.AddCommentAsync(400, new AddCommentRequest(100, "Recent comment"));

            var result = await _service.GetCommentsAsync(400);

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
            var addResult = await _service.AddCommentAsync(400, new AddCommentRequest(100, "To be deleted"));
            var commentId = addResult.Comments![0].Id;

            var result = await _service.DeleteCommentAsync(400, commentId);

            Assert.True(result.Success);
            Assert.Empty(result.Error);

            var comment = await DbContext.Comments.FindAsync(commentId);
            Assert.Null(comment);
        }

        [Fact]
        public async Task DeleteCommentAsync_NonExistentSubmission_ReturnsError()
        {
            var result = await _service.DeleteCommentAsync(99999, 1);

            Assert.False(result.Success);
            Assert.Equal("Submission not found.", result.Error);
        }

        [Fact]
        public async Task DeleteCommentAsync_NonExistentComment_ReturnsError()
        {
            var result = await _service.DeleteCommentAsync(400, 99999);

            Assert.False(result.Success);
            Assert.Equal("Comment not found.", result.Error);
        }

        [Fact]
        public async Task DeleteCommentAsync_RemovesFromSubmissionComments()
        {
            await _service.AddCommentAsync(400, new AddCommentRequest(100, "Comment 1"));
            var result2 = await _service.AddCommentAsync(400, new AddCommentRequest(101, "Comment 2"));
            var commentToDelete = result2.Comments![1].Id;

            await _service.DeleteCommentAsync(400, commentToDelete);

            var remaining = await _service.GetCommentsAsync(400);
            Assert.Single(remaining.Comments!);
        }

        [Fact]
        public async Task CommentOperations_ArePersisted()
        {
            var request = new AddCommentRequest(100, "Persistent comment");

            var addResult = await _service.AddCommentAsync(400, request);
            var commentId = addResult.Comments![0].Id;

            // Re-query from database
            var getResult = await _service.GetCommentsAsync(400);

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
            var longText = new string('X', 1000);
            var request = new AddCommentRequest(100, longText);

            var result = await _service.AddCommentAsync(400, request);

            Assert.True(result.Success);
            Assert.Equal(longText, result.Comments![0].Text);
        }
    }
}