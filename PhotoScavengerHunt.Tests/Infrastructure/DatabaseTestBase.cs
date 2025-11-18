using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Features.Tasks;
using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Features.Photos;
using Moq;

namespace PhotoScavengerHunt.Tests.Infrastructure
{
    // Base class for tests that provides in-memory database setup
    public abstract class DatabaseTestBase : IDisposable
    {
        protected PhotoScavengerHuntDbContext DbContext { get; private set; }
        
        protected DatabaseTestBase()
        {
            // Use a unique database name for each test to ensure isolation
            var options = new DbContextOptionsBuilder<PhotoScavengerHuntDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            DbContext = new PhotoScavengerHuntDbContext(options);
        }

        // Seeds the database with predictable test data
        protected void SeedTestData()
        {
            // Clear any existing data first
            DbContext.Comments.RemoveRange(DbContext.Comments);
            DbContext.Photos.RemoveRange(DbContext.Photos);
            DbContext.ChallengeParticipants.RemoveRange(DbContext.ChallengeParticipants);
            DbContext.Challenges.RemoveRange(DbContext.Challenges);
            DbContext.Tasks.RemoveRange(DbContext.Tasks);
            DbContext.Users.RemoveRange(DbContext.Users);
            DbContext.SaveChanges();

            var users = new[]
            {
                new UserProfile { Id = 100, Name = "TestUser1", Age = 25, Email = "test1@test.com", IsRegistered = true, PasswordHash = "hash1" },
                new UserProfile { Id = 101, Name = "TestUser2", Age = 30, Email = "test2@test.com", IsRegistered = true, PasswordHash = "hash2" },
                new UserProfile { Id = 102, Name = "TestUser3", Age = 35, Email = "test3@test.com", IsRegistered = false, PasswordHash = "" }
            };

            var tasks = new[]
            {
                new HuntTask { Id = 200, Description = "Test Task 1", AuthorId = 100 },
                new HuntTask { Id = 201, Description = "Test Task 2", AuthorId = 101 }
            };

            var challenges = new[]
            {
                new Challenge { Id = 300, Name = "Test Challenge 1", TaskId = 200, JoinCode = "TEST01", CreatorId = 100, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsPrivate = false },
                new Challenge { Id = 301, Name = "Test Challenge 2", TaskId = 201, JoinCode = "TEST02", CreatorId = 101, CreatedAt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc), IsPrivate = true }
            };

            var photos = new[]
            {
                new PhotoSubmission { Id = 400, TaskId = 200, UserId = 100, PhotoUrl = "/test/photo1.jpg", Votes = 5 },
                new PhotoSubmission { Id = 401, TaskId = 200, UserId = 101, PhotoUrl = "/test/photo2.jpg", Votes = 3 }
            };

            var challengeParticipants = new[]
            {
                new ChallengeParticipant { Id = 500, ChallengeId = 300, UserId = 100, Role = ChallengeRole.Admin, JoinedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new ChallengeParticipant { Id = 501, ChallengeId = 301, UserId = 101, Role = ChallengeRole.Admin, JoinedAt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc) }
            };

            DbContext.Users.AddRange(users);
            DbContext.SaveChanges();
            
            DbContext.Tasks.AddRange(tasks);
            DbContext.SaveChanges();
            
            DbContext.Challenges.AddRange(challenges);
            DbContext.SaveChanges();
            
            DbContext.ChallengeParticipants.AddRange(challengeParticipants);
            DbContext.SaveChanges();
            
            DbContext.Photos.AddRange(photos);
            DbContext.SaveChanges();
        }

        protected Mock<ILogger<T>> CreateMockLogger<T>()
        {
            return new Mock<ILogger<T>>();
        }

        public virtual void Dispose()
        {
            DbContext?.Database.EnsureDeleted();
            DbContext?.Dispose();
        }
    }
}