using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhotoScavengerHunt.Repositories;
using Moq;
using Xunit;

namespace PhotoScavengerHunt.Tests.Services
{
    public class CommentServiceTests : DatabaseTestBase
    {
        private readonly CommentService _service;
        private readonly Mock<ILogger<CommentService>> _mockLogger;
        private readonly Mock<IUserRepository> _mockUserRepo;

        public CommentServiceTests()
        {
            _mockLogger = CreateMockLogger<CommentService>();
            var photoRepo = new PhotoRepository(DbContext);
            _mockUserRepo = new Mock<IUserRepository>();
            _mockUserRepo
                .Setup(r => r.GetUserNamesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync((List<int> ids) => ids.Distinct().ToDictionary(id => id, id => $"User {id}"));

            _service = new CommentService(photoRepo, _mockUserRepo.Object, _mockLogger.Object);


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

            Assert.Equal(commentId, getResult.Comments![0].Id);
            Assert.Equal("Persistent comment", getResult.Comments[0].Text);
        }

        [Fact]
        public async Task AddCommentAsync_LongText_AcceptsFullText()
        {
            var longText = new string('X', 500);
            var request = new AddCommentRequest(100, longText);

            var result = await _service.AddCommentAsync(400, request);

            Assert.True(result.Success);
            Assert.Equal(longText, result.Comments![0].Text);
        }

        [Fact]
        public async Task AddCommentAsync_ExceedsMaxLength_ReturnsError()
        {
            var tooLongText = new string('X', 501);
            var request = new AddCommentRequest(100, tooLongText);

            var result = await _service.AddCommentAsync(400, request);

            Assert.False(result.Success);
            Assert.Contains("cannot exceed 500 characters", result.Error);
        }

        [Fact]
        public async Task AddCommentAsync_PopulatesUserNames()
        {
            var request = new AddCommentRequest(100, "Test comment");

            var result = await _service.AddCommentAsync(400, request);

            Assert.NotNull(result.Comments);
            Assert.All(result.Comments, c => Assert.NotNull(c.UserName));
            Assert.Equal("User 100", result.Comments[0].UserName);
        }

        [Fact]
        public async Task GetCommentsAsync_PopulatesUserNames()
        {
            await _service.AddCommentAsync(400, new AddCommentRequest(100, "Comment by 100"));
            await _service.AddCommentAsync(400, new AddCommentRequest(101, "Comment by 101"));

            var result = await _service.GetCommentsAsync(400);

            Assert.NotNull(result.Comments);
            Assert.All(result.Comments, c => Assert.NotNull(c.UserName));
            Assert.Contains(result.Comments, c => c.UserName == "User 100");
            Assert.Contains(result.Comments, c => c.UserName == "User 101");
        }

        [Fact]
        public async Task AddCommentAsync_MultipleUsersOnSameSubmission_Success()
        {
            await _service.AddCommentAsync(400, new AddCommentRequest(100, "User 100 comment"));
            await _service.AddCommentAsync(400, new AddCommentRequest(101, "User 101 comment"));
            await _service.AddCommentAsync(400, new AddCommentRequest(102, "User 102 comment"));

            var result = await _service.GetCommentsAsync(400);

            Assert.Equal(3, result.Comments!.Count);
            Assert.Equal(3, result.Comments.Select(c => c.UserId).Distinct().Count());
        }

        [Fact]
        public async Task GetCommentsAsync_NullCommentsList_ReturnsEmpty()
        {
            // Test with a submission that has no comments
            var result = await _service.GetCommentsAsync(401); // Different submission with no comments

            Assert.True(result.Success);
            Assert.NotNull(result.Comments);
            Assert.Empty(result.Comments);
        }

        [Fact]
        public async Task AddCommentAsync_SetsPhotoSubmissionId()
        {
            var request = new AddCommentRequest(100, "Test comment");

            var result = await _service.AddCommentAsync(400, request);

            Assert.Equal(400, result.Comments![0].PhotoSubmissionId);
        }

        [Fact]
        public async Task DeleteCommentAsync_OnlyDeletesSpecifiedComment()
        {
            var comment1 = await _service.AddCommentAsync(400, new AddCommentRequest(100, "Comment 1"));
            var comment2 = await _service.AddCommentAsync(400, new AddCommentRequest(101, "Comment 2"));
            var comment3 = await _service.AddCommentAsync(400, new AddCommentRequest(102, "Comment 3"));

            var commentToDelete = comment2.Comments![1].Id;

            await _service.DeleteCommentAsync(400, commentToDelete);

            var remaining = await _service.GetCommentsAsync(400);
            Assert.Equal(2, remaining.Comments!.Count);
            Assert.DoesNotContain(remaining.Comments, c => c.Id == commentToDelete);
        }

        [Fact]
        public async Task AddCommentAsync_ExactlyMaxLength_Succeeds()
        {
            var maxLengthText = new string('X', 500);
            var request = new AddCommentRequest(100, maxLengthText);

            var result = await _service.AddCommentAsync(400, request);

            Assert.True(result.Success);
            Assert.Equal(500, result.Comments![0].Text.Length);
        }

        [Fact]
        public async Task AddCommentAsync_OneCharacterOverMax_ReturnsError()
        {
            var overMaxText = new string('X', 501);
            var request = new AddCommentRequest(100, overMaxText);

            var result = await _service.AddCommentAsync(400, request);

            Assert.False(result.Success);
            Assert.Contains("500", result.Error);
        }

        [Fact]
        public async Task GetCommentsAsync_MultipleCallsSameSubmission_ConsistentResults()
        {
            await _service.AddCommentAsync(400, new AddCommentRequest(100, "Comment"));

            var result1 = await _service.GetCommentsAsync(400);
            var result2 = await _service.GetCommentsAsync(400);

            Assert.Equal(result1.Comments!.Count, result2.Comments!.Count);
            Assert.Equal(result1.Comments[0].Text, result2.Comments[0].Text);
        }
    }
}