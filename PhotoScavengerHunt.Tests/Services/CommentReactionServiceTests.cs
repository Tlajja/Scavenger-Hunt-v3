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
    public class CommentReactionServiceTests : DatabaseTestBase
    {
        private readonly CommentReactionService _service;
        private readonly Mock<ILogger<CommentReactionService>> _mockLogger;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private int _testCommentId;

        public CommentReactionServiceTests()
        {
            _mockLogger = CreateMockLogger<CommentReactionService>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockUserRepo
                .Setup(r => r.GetUserNamesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync((List<int> ids) => ids.Distinct().ToDictionary(id => id, id => $"User {id}"));

            _service = new CommentReactionService(DbContext, _mockUserRepo.Object, _mockLogger.Object);

            SeedTestData();
            SeedTestComment();
        }

        private void SeedTestComment()
        {
            var comment = new Comment
            {
                Id = 600,
                PhotoSubmissionId = 400,
                UserId = 100,
                Text = "Test comment for reactions",
                Timestamp = DateTime.UtcNow
            };
            DbContext.Comments.Add(comment);
            DbContext.SaveChanges();
            _testCommentId = comment.Id;
        }

        [Fact]
        public async Task AddReactionAsync_ValidRequest_ReturnsSuccess()
        {
            var result = await _service.AddReactionAsync(_testCommentId, 100, "👍");

            Assert.True(result.Success);
            Assert.Empty(result.Error);
            Assert.NotNull(result.Reactions);
            Assert.Single(result.Reactions);
            Assert.Equal("👍", result.Reactions[0].Emoji);
            Assert.Equal(100, result.Reactions[0].UserId);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task AddReactionAsync_EmptyEmoji_ReturnsError(string? emoji)
        {
            var result = await _service.AddReactionAsync(_testCommentId, 100, emoji!);

            Assert.False(result.Success);
            Assert.Equal("Emoji cannot be empty.", result.Error);
            Assert.Null(result.Reactions);
        }

        [Fact]
        public async Task AddReactionAsync_EmojiTooLong_ReturnsError()
        {
            var longEmoji = "😀😀😀😀😀😀😀😀😀😀😀"; // 11 emojis
            var result = await _service.AddReactionAsync(_testCommentId, 100, longEmoji);

            Assert.False(result.Success);
            Assert.Equal("Emoji is too long.", result.Error);
            Assert.Null(result.Reactions);
        }

        [Fact]
        public async Task AddReactionAsync_NonExistentComment_ReturnsError()
        {
            var result = await _service.AddReactionAsync(99999, 100, "👍");

            Assert.False(result.Success);
            Assert.Equal("Comment not found.", result.Error);
        }

        [Fact]
        public async Task AddReactionAsync_DuplicateReaction_ReturnsError()
        {
            await _service.AddReactionAsync(_testCommentId, 100, "👍");
            var result = await _service.AddReactionAsync(_testCommentId, 100, "👍");

            Assert.False(result.Success);
            Assert.Equal("You have already reacted with this emoji.", result.Error);
        }

        [Fact]
        public async Task AddReactionAsync_DifferentEmoji_ReplacesOldReaction()
        {
            await _service.AddReactionAsync(_testCommentId, 100, "👍");
            var result = await _service.AddReactionAsync(_testCommentId, 100, "❤️");

            Assert.True(result.Success);
            Assert.Single(result.Reactions);
            Assert.Equal("❤️", result.Reactions![0].Emoji);
            
            // Verify old reaction was removed
            var allReactions = await DbContext.CommentReactions
                .Where(r => r.CommentId == _testCommentId && r.UserId == 100)
                .ToListAsync();
            Assert.Single(allReactions);
            Assert.Equal("❤️", allReactions[0].Emoji);
        }

        [Fact]
        public async Task AddReactionAsync_MultipleUsers_AllowsSameEmoji()
        {
            await _service.AddReactionAsync(_testCommentId, 100, "👍");
            await _service.AddReactionAsync(_testCommentId, 101, "👍");
            var result = await _service.AddReactionAsync(_testCommentId, 102, "👍");

            Assert.True(result.Success);
            Assert.Equal(3, result.Reactions!.Count);
            Assert.All(result.Reactions, r => Assert.Equal("👍", r.Emoji));
        }

        [Fact]
        public async Task AddReactionAsync_SetsCreatedAt()
        {
            var beforeAdd = DateTime.UtcNow.AddSeconds(-1);
            var result = await _service.AddReactionAsync(_testCommentId, 100, "👍");
            var afterAdd = DateTime.UtcNow.AddSeconds(1);

            var reaction = result.Reactions![0];
            Assert.True(reaction.CreatedAt >= beforeAdd);
            Assert.True(reaction.CreatedAt <= afterAdd);
        }

        [Fact]
        public async Task AddReactionAsync_PopulatesUserNames()
        {
            var result = await _service.AddReactionAsync(_testCommentId, 100, "👍");

            Assert.NotNull(result.Reactions);
            Assert.All(result.Reactions, r => Assert.NotNull(r.UserName));
            Assert.Equal("User 100", result.Reactions[0].UserName);
        }

        [Fact]
        public async Task RemoveReactionAsync_ValidReaction_ReturnsSuccess()
        {
            await _service.AddReactionAsync(_testCommentId, 100, "👍");
            var result = await _service.RemoveReactionAsync(_testCommentId, 100, "👍");

            Assert.True(result.Success);
            Assert.Empty(result.Error);

            var reaction = await DbContext.CommentReactions
                .FirstOrDefaultAsync(r => r.CommentId == _testCommentId && r.UserId == 100 && r.Emoji == "👍");
            Assert.Null(reaction);
        }

        [Fact]
        public async Task RemoveReactionAsync_NonExistentReaction_ReturnsError()
        {
            var result = await _service.RemoveReactionAsync(_testCommentId, 100, "👍");

            Assert.False(result.Success);
            Assert.Equal("Reaction not found.", result.Error);
        }

        [Fact]
        public async Task RemoveReactionAsync_OnlyRemovesSpecificReaction()
        {
            await _service.AddReactionAsync(_testCommentId, 100, "👍");
            await _service.AddReactionAsync(_testCommentId, 101, "👍");
            await _service.AddReactionAsync(_testCommentId, 102, "❤️");

            await _service.RemoveReactionAsync(_testCommentId, 100, "👍");

            var remaining = await _service.GetReactionsAsync(_testCommentId);
            Assert.Equal(2, remaining.Reactions!.Count);
            Assert.DoesNotContain(remaining.Reactions, r => r.UserId == 100);
        }

        [Fact]
        public async Task GetReactionsAsync_ValidComment_ReturnsReactions()
        {
            await _service.AddReactionAsync(_testCommentId, 100, "👍");
            await _service.AddReactionAsync(_testCommentId, 101, "❤️");

            var result = await _service.GetReactionsAsync(_testCommentId);

            Assert.True(result.Success);
            Assert.NotNull(result.Reactions);
            Assert.Equal(2, result.Reactions.Count);
        }

        [Fact]
        public async Task GetReactionsAsync_NoReactions_ReturnsEmpty()
        {
            var result = await _service.GetReactionsAsync(_testCommentId);

            Assert.True(result.Success);
            Assert.NotNull(result.Reactions);
            Assert.Empty(result.Reactions);
        }

        [Fact]
        public async Task GetReactionsAsync_PopulatesUserNames()
        {
            await _service.AddReactionAsync(_testCommentId, 100, "👍");
            await _service.AddReactionAsync(_testCommentId, 101, "❤️");

            var result = await _service.GetReactionsAsync(_testCommentId);

            Assert.NotNull(result.Reactions);
            Assert.All(result.Reactions, r => Assert.NotNull(r.UserName));
            Assert.Contains(result.Reactions, r => r.UserName == "User 100");
            Assert.Contains(result.Reactions, r => r.UserName == "User 101");
        }

        [Fact]
        public async Task ReactionOperations_ArePersisted()
        {
            await _service.AddReactionAsync(_testCommentId, 100, "👍");

            // Re-query from database
            var result = await _service.GetReactionsAsync(_testCommentId);

            Assert.Single(result.Reactions!);
            Assert.Equal("👍", result.Reactions[0].Emoji);
            Assert.Equal(100, result.Reactions[0].UserId);
        }

        [Fact]
        public async Task AddReactionAsync_MultipleEmojis_AllStored()
        {
            await _service.AddReactionAsync(_testCommentId, 100, "👍");
            await _service.AddReactionAsync(_testCommentId, 101, "❤️");
            await _service.AddReactionAsync(_testCommentId, 102, "😂");

            var result = await _service.GetReactionsAsync(_testCommentId);

            Assert.Equal(3, result.Reactions!.Count);
            Assert.Contains(result.Reactions, r => r.Emoji == "👍");
            Assert.Contains(result.Reactions, r => r.Emoji == "❤️");
            Assert.Contains(result.Reactions, r => r.Emoji == "😂");
        }

        [Fact]
        public async Task AddReactionAsync_UnicodeEmoji_HandledCorrectly()
        {
            var emojis = new[] { "👍", "❤️", "😂", "🎉", "🔥" };

            foreach (var emoji in emojis)
            {
                var result = await _service.AddReactionAsync(_testCommentId, 100 + Array.IndexOf(emojis, emoji), emoji);
                Assert.True(result.Success);
            }

            var allReactions = await _service.GetReactionsAsync(_testCommentId);
            Assert.Equal(emojis.Length, allReactions.Reactions!.Count);
        }

        [Fact]
        public async Task RemoveReactionAsync_LastReaction_LeavesCommentIntact()
        {
            await _service.AddReactionAsync(_testCommentId, 100, "👍");
            await _service.RemoveReactionAsync(_testCommentId, 100, "👍");

            var comment = await DbContext.Comments.FindAsync(_testCommentId);
            Assert.NotNull(comment);
            Assert.Equal("Test comment for reactions", comment.Text);
        }

        [Fact]
        public async Task AddReactionAsync_CascadeDelete_RemovesWithComment()
        {
            await _service.AddReactionAsync(_testCommentId, 100, "👍");
            await _service.AddReactionAsync(_testCommentId, 101, "❤️");

            // Delete the comment
            var comment = await DbContext.Comments.FindAsync(_testCommentId);
            DbContext.Comments.Remove(comment!);
            await DbContext.SaveChangesAsync();

            // Verify reactions were cascade deleted
            var reactions = await DbContext.CommentReactions
                .Where(r => r.CommentId == _testCommentId)
                .ToListAsync();
            Assert.Empty(reactions);
        }

        [Fact]
        public async Task AddReactionAsync_SameUserDifferentComments_AllowsSameEmoji()
        {
            // Create second comment
            var comment2 = new Comment
            {
                PhotoSubmissionId = 400,
                UserId = 101,
                Text = "Second test comment",
                Timestamp = DateTime.UtcNow
            };
            DbContext.Comments.Add(comment2);
            await DbContext.SaveChangesAsync();

            await _service.AddReactionAsync(_testCommentId, 100, "👍");
            var result = await _service.AddReactionAsync(comment2.Id, 100, "👍");

            Assert.True(result.Success);
            
            var allUserReactions = await DbContext.CommentReactions
                .Where(r => r.UserId == 100 && r.Emoji == "👍")
                .ToListAsync();
            Assert.Equal(2, allUserReactions.Count);
        }

        [Fact]
        public async Task GetReactionsAsync_MultipleCallsSameComment_ConsistentResults()
        {
            await _service.AddReactionAsync(_testCommentId, 100, "👍");

            var result1 = await _service.GetReactionsAsync(_testCommentId);
            var result2 = await _service.GetReactionsAsync(_testCommentId);

            Assert.Equal(result1.Reactions!.Count, result2.Reactions!.Count);
            Assert.Equal(result1.Reactions[0].Emoji, result2.Reactions[0].Emoji);
        }

        [Fact]
        public async Task AddReactionAsync_MaxLengthEmoji_Succeeds()
        {
            var maxLengthEmoji = "1234567890"; // 10 characters
            var result = await _service.AddReactionAsync(_testCommentId, 100, maxLengthEmoji);

            Assert.True(result.Success);
            Assert.Equal(maxLengthEmoji, result.Reactions![0].Emoji);
        }

        [Fact]
        public async Task RemoveReactionAsync_WrongUser_ReturnsError()
        {
            await _service.AddReactionAsync(_testCommentId, 100, "👍");
            var result = await _service.RemoveReactionAsync(_testCommentId, 101, "👍");

            Assert.False(result.Success);
            Assert.Equal("Reaction not found.", result.Error);
        }

        [Fact]
        public async Task RemoveReactionAsync_WrongEmoji_ReturnsError()
        {
            await _service.AddReactionAsync(_testCommentId, 100, "👍");
            var result = await _service.RemoveReactionAsync(_testCommentId, 100, "❤️");

            Assert.False(result.Success);
            Assert.Equal("Reaction not found.", result.Error);
        }

        [Fact]
        public async Task AddReactionAsync_ReturnsAllReactionsForComment()
        {
            await _service.AddReactionAsync(_testCommentId, 100, "👍");
            await _service.AddReactionAsync(_testCommentId, 101, "❤️");
            var result = await _service.AddReactionAsync(_testCommentId, 102, "😂");

            Assert.Equal(3, result.Reactions!.Count);
        }

        [Fact]
        public async Task AddReactionAsync_UserChangesReaction_OnlyOneReactionRemains()
        {
            await _service.AddReactionAsync(_testCommentId, 100, "👍");
            await _service.AddReactionAsync(_testCommentId, 100, "❤️");
            await _service.AddReactionAsync(_testCommentId, 100, "😂");

            var allReactions = await DbContext.CommentReactions
                .Where(r => r.CommentId == _testCommentId && r.UserId == 100)
                .ToListAsync();

            Assert.Single(allReactions);
            Assert.Equal("😂", allReactions[0].Emoji);
        }
    }
}
