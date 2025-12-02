using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Tests.Infrastructure;
using PhotoScavengerHunt.Exceptions;
using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Repositories;
using Moq;
using PhotoScavengerHunt.Services.Interfaces;

namespace PhotoScavengerHunt.Tests.Services
{
    public class ChallengeServiceTests : DatabaseTestBase
    {
        private readonly ChallengeService _service;

        public ChallengeServiceTests()
        {
            var photoRepo = new PhotoRepository(DbContext);
            var mockStorage = new Mock<IStorageService>();
            mockStorage.Setup(s => s.DeleteFileAsync(It.IsAny<string>())).Returns(() => Task.CompletedTask);

            _service =  new ChallengeService(
                        new ChallengeRepository(DbContext),
                        new UserRepository(DbContext),
                        new TaskRepository(DbContext),
                        new ChallengeParticipantRepository(DbContext), 
                        photoRepo,
                        mockStorage.Object
                    );

            SeedTestData();
        }

        [Fact]
        public async Task CreateChallengeAsync_ValidRequest_ReturnsChallenge()
        {
            // Arrange
            var request = new CreateChallengeRequest(
                "New Challenge",
                creatorId: 102,
                taskIds: new[] { 200 },
                deadline: DateTime.UtcNow.AddDays(3),
                isPrivate: false
            );

            // Act
            var challenge = await _service.CreateChallengeAsync(request);

            // Assert
            Assert.NotNull(challenge);
            Assert.Equal("New Challenge", challenge.Name);
            Assert.Contains(challenge.ChallengeTasks, ct => ct.TaskId == 200);
            Assert.Equal(102, challenge.CreatorId);
            Assert.NotEmpty(challenge.JoinCode);
        }

        [Fact]
        public async Task CreateChallengeAsync_EmptyName_ThrowsValidationException()
        {
            // Arrange
            var request = new CreateChallengeRequest
            ("",
             taskIds: new[] { 200 }, 
             creatorId: 100, 
             deadline: DateTime.UtcNow.AddDays(3)
            );

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateChallengeAsync(request)
            );
        }

        [Fact]
        public async Task CreateChallengeAsync_NonExistentTask_ThrowsNotFoundException()
        {
            // Arrange
            var request = new CreateChallengeRequest("Test", taskIds: new[] { 99999 }, creatorId: 100, deadline: DateTime.UtcNow.AddDays(3));

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _service.CreateChallengeAsync(request)
            );
        }

        [Fact]
        public async Task CreateChallengeAsync_UserAlreadyHasChallenge_ThrowsLimitException()
        {
            // Arrange - user 100 already has challenge 300 as admin
            var request = new CreateChallengeRequest("Another", taskIds: new[] { 201 }, creatorId: 100, deadline: DateTime.UtcNow.AddDays(3));

            // Act & Assert
            await Assert.ThrowsAsync<LimitExceededException>(
                () => _service.CreateChallengeAsync(request)
            );
        }

        [Fact]
        public async Task CreateChallengeAsync_PastDeadline_ThrowsValidationException()
        {
            // Arrange
            var request = new CreateChallengeRequest("Test", taskIds: new[] { 200 }, creatorId: 102, deadline: DateTime.UtcNow.AddDays(-1));

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateChallengeAsync(request)
            );
        }

        [Fact]
        public async Task CreateChallengeAsync_DeadlineTooFar_ThrowsValidationException()
        {
            // Arrange
            var request = new CreateChallengeRequest("Test", taskIds: new[] { 200 }, creatorId: 102, deadline: DateTime.UtcNow.AddDays(8));

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateChallengeAsync(request)
            );
        }

        [Fact]
        public async Task JoinChallengeAsync_ValidCode_ReturnsParticipant()
        {
            // Arrange
            var request = new JoinChallengeRequest("TEST01", 102);

            // Act
            var participant = await _service.JoinChallengeAsync(request);

            // Assert
            Assert.NotNull(participant);
            Assert.Equal(300, participant.ChallengeId);
            Assert.Equal(102, participant.UserId);
            Assert.Equal(ChallengeRole.Participant, participant.Role);
        }

        [Fact]
        public async Task JoinChallengeAsync_EmptyCode_ThrowsValidationException()
        {
            // Arrange
            var request = new JoinChallengeRequest("", 102);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.JoinChallengeAsync(request)
            );
        }

        [Fact]
        public async Task JoinChallengeAsync_InvalidCode_ThrowsNotFoundException()
        {
            // Arrange
            var request = new JoinChallengeRequest("INVALID", 102);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _service.JoinChallengeAsync(request)
            );
        }

        [Fact]
        public async Task JoinChallengeAsync_AlreadyMember_ThrowsValidationException()
        {
            // Arrange - user 100 is already admin of challenge 300
            var request = new JoinChallengeRequest("TEST01", 100);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.JoinChallengeAsync(request)
            );
        }

        [Fact]
        public async Task GetChallengesAsync_PublicOnly_ReturnsOnlyPublic()
        {
            // Act
            var challenges = await _service.GetChallengesAsync(publicOnly: true);

            // Assert
            Assert.All(challenges, c => Assert.False(c.IsPrivate));
        }

        [Fact]
        public async Task GetChallengeByIdAsync_ValidId_ReturnsWithParticipants()
        {
            // Act
            var challenge = await _service.GetChallengeByIdAsync(300);

            // Assert
            Assert.NotNull(challenge);
            Assert.NotNull(challenge.Participants);
            Assert.NotEmpty(challenge.Participants);
        }

        [Fact]
        public async Task DeleteChallengeAsync_AdminUser_DeletesSuccessfully()
        {
            // Act
            await _service.DeleteChallengeAsync(300, 100);

            // Assert
            var challenge = await DbContext.Challenges.FindAsync(300);
            Assert.Null(challenge);
        }

        [Fact]
        public async Task DeleteChallengeAsync_NonAdmin_ThrowsValidationException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.DeleteChallengeAsync(300, 102)
            );
        }

        [Fact]
        public async Task LeaveChallengeAsync_ParticipantLeaves_RemovesParticipant()
        {
            // Arrange
            await _service.JoinChallengeAsync(new JoinChallengeRequest("TEST01", 102));

            // Act
            await _service.LeaveChallengeAsync(300, 102);

            // Assert
            var participant = await DbContext.ChallengeParticipants
                .FirstOrDefaultAsync(cp => cp.ChallengeId == 300 && cp.UserId == 102);
            Assert.Null(participant);
        }

        [Fact]
        public async Task LeaveChallengeAsync_AdminLeavesAlone_DeletesChallenge()
        {
            // Act
            await _service.LeaveChallengeAsync(300, 100);

            // Assert
            var challenge = await DbContext.Challenges.FindAsync(300);
            Assert.Null(challenge);
        }

        [Fact]
        public async Task AdvanceChallengeAsync_OpenToClosed_ChangesStatus()
        {
            // Act
            var challenge = await _service.AdvanceChallengeAsync(300, 100);

            // Assert
            Assert.Equal(ChallengeStatus.Closed, challenge.Status);
        }

        [Fact]
        public async Task AdvanceChallengeAsync_NonAdmin_ThrowsValidationException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.AdvanceChallengeAsync(300, 102)
            );
        }

        [Fact]
        public async Task FinalizeChallengeAsync_DeterminesWinner()
        {
            // Arrange - Add photos for challenge 300
            DbContext.Photos.Add(new PhotoScavengerHunt.Features.Photos.PhotoSubmission
            {
                TaskId = 200,
                UserId = 100,
                ChallengeId = 300,
                PhotoUrl = "/test.jpg",
                Votes = 10
            });
            DbContext.Photos.Add(new PhotoScavengerHunt.Features.Photos.PhotoSubmission
            {
                TaskId = 200,
                UserId = 101,
                ChallengeId = 300,
                PhotoUrl = "/test2.jpg",
                Votes = 5
            });
            await DbContext.SaveChangesAsync();

            // Act
            var challenge = await _service.FinalizeChallengeAsync(300);

            // Assert
            Assert.Equal(ChallengeStatus.Completed, challenge.Status);
            Assert.Equal(100, challenge.WinnerId);

            var winner = await DbContext.Users.FindAsync(100);
            Assert.Equal(1, winner!.Wins);
        }

        [Fact]
        public async Task GetChallengesForUserAsync_ReturnsUserChallenges()
        {
            // Act
            var challenges = await _service.GetChallengesForUserAsync(100);

            // Assert
            Assert.NotEmpty(challenges);
            Assert.All(challenges, c => Assert.NotNull(c.Participants));
        }
    }
}