using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Leaderboard;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;


namespace PhotoScavengerHunt.Tests.Services
{
    public class LeaderboardServiceTests : DatabaseTestBase
    {
        private readonly LeaderboardService _service;
        private readonly Mock<ILogger<LeaderboardService>> _mockLogger;

        public LeaderboardServiceTests()
        {
            _mockLogger = CreateMockLogger<LeaderboardService>();
            _service = new LeaderboardService(DbContext, _mockLogger.Object);
            SeedTestData();
        }

        [Fact]
        public async Task GetLeaderboardAsync_ReturnsEntriesSortedByVotes()
        {
            // Act
            var result = await _service.GetLeaderboardAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Seed data has 2 users with submissions
            
            // Should be sorted by votes descending (user 100 has 5, user 101 has 3)
            Assert.Equal(100, result[0].UserId);
            Assert.Equal(5, result[0].TotalVotes);
            Assert.Equal(101, result[1].UserId);
            Assert.Equal(3, result[1].TotalVotes);
        }

        [Fact]
        public async Task GetLeaderboardAsync_IncludesUserNames()
        {
            // Act
            var result = await _service.GetLeaderboardAsync();

            // Assert
            Assert.Equal("TestUser1", result[0].UserName);
            Assert.Equal("TestUser2", result[1].UserName);
        }

        [Fact]
        public async Task GetLeaderboardAsync_EmptyDatabase_ReturnsEmpty()
        {
            // Arrange - Clear all photos
            DbContext.Photos.RemoveRange(DbContext.Photos);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _service.GetLeaderboardAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetLeaderboardAsync_AggregatesMultipleSubmissions()
        {
            // Arrange - Add another submission for user 100
            DbContext.Photos.Add(new PhotoSubmission
            {
                TaskId = 201,
                UserId = 100,
                PhotoUrl = "/test/photo3.jpg",
                Votes = 10
            });
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _service.GetLeaderboardAsync();

            // Assert
            var user100Entry = result.First(e => e.UserId == 100);
            Assert.Equal(15, user100Entry.TotalVotes); // 5 + 10
        }

        [Fact]
        public async Task GetLeaderboardAsync_UnknownUser_ShowsUnknown()
        {
            // Arrange - Add submission for non-existent user
            DbContext.Photos.Add(new PhotoSubmission
            {
                TaskId = 200,
                UserId = 99999,
                PhotoUrl = "/test/photo_unknown.jpg",
                Votes = 1
            });
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _service.GetLeaderboardAsync();

            // Assert
            var unknownEntry = result.First(e => e.UserId == 99999);
            Assert.Equal("Unknown", unknownEntry.UserName);
        }

        [Fact]
        public async Task GetLeaderboardAsync_SameVotes_SortsByName()
        {
            // Arrange - Make both users have same votes
            var photo1 = await DbContext.Photos.FindAsync(400);
            var photo2 = await DbContext.Photos.FindAsync(401);
            photo1!.Votes = 5;
            photo2!.Votes = 5;
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _service.GetLeaderboardAsync();

            // Assert
            Assert.Equal(2, result.Count);
            // Should be sorted alphabetically when votes are equal
            Assert.True(string.Compare(
                result[0].UserName, 
                result[1].UserName, 
                StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public void LeaderboardEntry_CompareTo_SortsCorrectly()
        {
            // Arrange
            var entries = new List<LeaderboardEntry>
            {
                new LeaderboardEntry(1, "Alice", 10),
                new LeaderboardEntry(2, "Bob", 15),
                new LeaderboardEntry(3, "Charlie", 10),
                new LeaderboardEntry(4, "David", 20)
            };

            // Act
            entries.Sort();

            // Assert
            Assert.Equal(4, entries[0].UserId); // David (20 votes)
            Assert.Equal(2, entries[1].UserId); // Bob (15 votes)
            Assert.Equal(1, entries[2].UserId); // Alice (10 votes, alphabetically before Charlie)
            Assert.Equal(3, entries[3].UserId); // Charlie (10 votes)
        }

        [Fact]
        public void LeaderboardEntry_CompareTo_SameNameAndVotes_SortsByUserId()
        {
            // Arrange
            var entries = new List<LeaderboardEntry>
            {
                new LeaderboardEntry(2, "Alice", 10),
                new LeaderboardEntry(1, "Alice", 10),
                new LeaderboardEntry(3, "Alice", 10)
            };

            // Act
            entries.Sort();

            // Assert
            Assert.Equal(1, entries[0].UserId);
            Assert.Equal(2, entries[1].UserId);
            Assert.Equal(3, entries[2].UserId);
        }

        [Fact]
        public async Task GetLeaderboardAsync_ZeroVotes_IncludedInLeaderboard()
        {
            // Arrange - Add submission with 0 votes
            DbContext.Photos.Add(new PhotoSubmission
            {
                TaskId = 200,
                UserId = 102,
                PhotoUrl = "/test/photo_zero.jpg",
                Votes = 0
            });
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _service.GetLeaderboardAsync();

            // Assert
            Assert.Contains(result, e => e.UserId == 102 && e.TotalVotes == 0);
        }
    }
}