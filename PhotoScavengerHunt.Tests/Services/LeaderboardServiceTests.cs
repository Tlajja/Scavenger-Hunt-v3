using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Leaderboard;
using PhotoScavengerHunt.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;


namespace PhotoScavengerHunt.Tests.Services
{
    public class LeaderboardServiceTests
    {
        private readonly LeaderboardService _service;
        private readonly Mock<ILeaderboardRepository> _mockRepo;
        private readonly Mock<ILogger<LeaderboardService>> _mockLogger;

        public LeaderboardServiceTests()
        {
            _mockRepo = new Mock<ILeaderboardRepository>();
            _mockLogger = new Mock<ILogger<LeaderboardService>>();
            _service = new LeaderboardService(_mockRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetLeaderboardAsync_ReturnsEntriesSortedByVotes()
        {
            _mockRepo.Setup(r => r.GetLeaderboardAsync()).ReturnsAsync(new List<LeaderboardEntry>
            {
                new LeaderboardEntry(100, "TestUser1", 5),
                new LeaderboardEntry(101, "TestUser2", 3)
            });

            var result = await _service.GetLeaderboardAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(100, result[0].UserId);
            Assert.Equal(5, result[0].TotalVotes);
            Assert.Equal(101, result[1].UserId);
            Assert.Equal(3, result[1].TotalVotes);
        }

        [Fact]
        public async Task GetLeaderboardAsync_IncludesUserNames()
        {
            _mockRepo.Setup(r => r.GetLeaderboardAsync()).ReturnsAsync(new List<LeaderboardEntry>
            {
                new LeaderboardEntry(100, "TestUser1", 5),
                new LeaderboardEntry(101, "TestUser2", 3)
            });

            var result = await _service.GetLeaderboardAsync();

            Assert.Equal("TestUser1", result[0].UserName);
            Assert.Equal("TestUser2", result[1].UserName);
        }

        [Fact]
        public async Task GetLeaderboardAsync_EmptyDatabase_ReturnsEmpty()
        {
            _mockRepo.Setup(r => r.GetLeaderboardAsync()).ReturnsAsync(new List<LeaderboardEntry>());

            var result = await _service.GetLeaderboardAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetLeaderboardAsync_AggregatesMultipleSubmissions()
        {
            _mockRepo.Setup(r => r.GetLeaderboardAsync()).ReturnsAsync(new List<LeaderboardEntry>
            {
                new LeaderboardEntry(100, "TestUser1", 15)
            });

            var result = await _service.GetLeaderboardAsync();

            var user100Entry = result.First(e => e.UserId == 100);
            Assert.Equal(15, user100Entry.TotalVotes);
        }

        [Fact]
        public async Task GetLeaderboardAsync_UnknownUser_ShowsUnknown()
        {
            _mockRepo.Setup(r => r.GetLeaderboardAsync()).ReturnsAsync(new List<LeaderboardEntry>
            {
                new LeaderboardEntry(99999, "Unknown", 1)
            });

            var result = await _service.GetLeaderboardAsync();

            var unknownEntry = result.First(e => e.UserId == 99999);
            Assert.Equal("Unknown", unknownEntry.UserName);
        }

        [Fact]
        public async Task GetLeaderboardAsync_SameVotes_SortsByName()
        {
            _mockRepo.Setup(r => r.GetLeaderboardAsync()).ReturnsAsync(new List<LeaderboardEntry>
            {
                new LeaderboardEntry(100, "Alice", 5),
                new LeaderboardEntry(101, "Bob", 5)
            });

            var result = await _service.GetLeaderboardAsync();

            Assert.Equal(2, result.Count);
            Assert.True(string.Compare(result[0].UserName, result[1].UserName, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public void LeaderboardEntry_CompareTo_SortsCorrectly()
        {
            var entries = new List<LeaderboardEntry>
            {
                new LeaderboardEntry(1, "Alice", 10),
                new LeaderboardEntry(2, "Bob", 15),
                new LeaderboardEntry(3, "Charlie", 10),
                new LeaderboardEntry(4, "David", 20)
            };

            entries.Sort();

            Assert.Equal(4, entries[0].UserId);
            Assert.Equal(2, entries[1].UserId);
            Assert.Equal(1, entries[2].UserId);
            Assert.Equal(3, entries[3].UserId);
        }

        [Fact]
        public void LeaderboardEntry_CompareTo_SameNameAndVotes_SortsByUserId()
        {
            var entries = new List<LeaderboardEntry>
            {
                new LeaderboardEntry(2, "Alice", 10),
                new LeaderboardEntry(1, "Alice", 10),
                new LeaderboardEntry(3, "Alice", 10)
            };

            entries.Sort();

            Assert.Equal(1, entries[0].UserId);
            Assert.Equal(2, entries[1].UserId);
            Assert.Equal(3, entries[2].UserId);
        }

        [Fact]
        public async Task GetLeaderboardAsync_ZeroVotes_IncludedInLeaderboard()
        {
            _mockRepo.Setup(r => r.GetLeaderboardAsync()).ReturnsAsync(new List<LeaderboardEntry>
            {
                new LeaderboardEntry(102, "ZeroUser", 0)
            });

            var result = await _service.GetLeaderboardAsync();

            Assert.Contains(result, e => e.UserId == 102 && e.TotalVotes == 0);
        }

        [Fact]
        public async Task GetLeaderboardAsync_RepositoryThrowsException_ThrowsApplicationException()
        {
            _mockRepo.Setup(r => r.GetLeaderboardAsync())
                .ThrowsAsync(new Exception("Database error"));

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _service.GetLeaderboardAsync());

            Assert.Equal("Unable to retrieve leaderboard data at this time.", exception.Message);
        }

        [Fact]
        public async Task GetHallOfFameAsync_DefaultTop10_ReturnsTopEntries()
        {
            var mockEntries = new List<LeaderboardEntry>
            {
                new LeaderboardEntry(1, "User1", 100),
                new LeaderboardEntry(2, "User2", 90),
                new LeaderboardEntry(3, "User3", 80),
                new LeaderboardEntry(4, "User4", 70),
                new LeaderboardEntry(5, "User5", 60),
                new LeaderboardEntry(6, "User6", 50),
                new LeaderboardEntry(7, "User7", 40),
                new LeaderboardEntry(8, "User8", 30),
                new LeaderboardEntry(9, "User9", 20),
                new LeaderboardEntry(10, "User10", 10)
            };

            _mockRepo.Setup(r => r.GetHallOfFameAsync(10))
                .ReturnsAsync(mockEntries);

            var result = await _service.GetHallOfFameAsync();

            Assert.NotNull(result);
            Assert.Equal(10, result.Count);
            Assert.Equal(100, result[0].TotalVotes);
            Assert.Equal(10, result[9].TotalVotes);
        }

        [Fact]
        public async Task GetHallOfFameAsync_CustomTop_ReturnsCorrectNumber()
        {
            var mockEntries = new List<LeaderboardEntry>
            {
                new LeaderboardEntry(1, "User1", 100),
                new LeaderboardEntry(2, "User2", 90),
                new LeaderboardEntry(3, "User3", 80),
                new LeaderboardEntry(4, "User4", 70),
                new LeaderboardEntry(5, "User5", 60)
            };

            _mockRepo.Setup(r => r.GetHallOfFameAsync(5))
                .ReturnsAsync(mockEntries);

            var result = await _service.GetHallOfFameAsync(5);

            Assert.NotNull(result);
            Assert.Equal(5, result.Count);
        }

        [Fact]
        public async Task GetHallOfFameAsync_EmptyDatabase_ReturnsEmpty()
        {
            _mockRepo.Setup(r => r.GetHallOfFameAsync(10))
                .ReturnsAsync(new List<LeaderboardEntry>());

            var result = await _service.GetHallOfFameAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetHallOfFameAsync_RepositoryThrowsException_Rethrows()
        {
            _mockRepo.Setup(r => r.GetHallOfFameAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetHallOfFameAsync());
        }

        [Fact]
        public async Task GetHallOfFameAsync_FewerEntriesThanRequested_ReturnsAll()
        {
            var mockEntries = new List<LeaderboardEntry>
            {
                new LeaderboardEntry(1, "User1", 100),
                new LeaderboardEntry(2, "User2", 90)
            };

            _mockRepo.Setup(r => r.GetHallOfFameAsync(10))
                .ReturnsAsync(mockEntries);

            var result = await _service.GetHallOfFameAsync(10);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
    }
}