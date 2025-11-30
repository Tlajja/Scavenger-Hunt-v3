// Replace DatabaseTestBase.cs with this improved version:

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Features.Tasks;
using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Features.Photos;
using Moq;

namespace PhotoScavengerHunt.Tests.Infrastructure
{
    /// <summary>
    /// Base class providing in-memory database for service tests.
    /// Each test gets isolated database instance to prevent test interference.
    /// 
    /// WHY THIS EXISTS:
    /// - DRY principle: All service tests need database setup
    /// - Isolation: Each test uses unique DB (Guid-based name)
    /// - Predictable data: SeedTestData() provides known state
    /// </summary>
    public abstract class DatabaseTestBase : IDisposable
    {
        protected PhotoScavengerHuntDbContext DbContext { get; private set; }
        
        protected DatabaseTestBase()
        {
            // Unique DB per test = no interference between tests
            var options = new DbContextOptionsBuilder<PhotoScavengerHuntDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            DbContext = new PhotoScavengerHuntDbContext(options);
        }

        /// <summary>
        /// Seeds predictable test data. Call this in test constructor.
        /// 
        /// DATA LAYOUT:
        /// Users: 100 (TestUser1), 101 (TestUser2), 102 (TestUser3-unregistered)
        /// Tasks: 200 (by user 100), 201 (by user 101)
        /// Challenges: 300 (task 200, admin: 100), 301 (task 201, admin: 101)
        /// Photos: 400 (task 200, user 100, 5 votes), 401 (task 200, user 101, 3 votes)
        /// </summary>
        protected void SeedTestData()
        {
            // Clear any existing data
            DbContext.Comments.RemoveRange(DbContext.Comments);
            DbContext.Photos.RemoveRange(DbContext.Photos);
            DbContext.ChallengeParticipants.RemoveRange(DbContext.ChallengeParticipants);
            DbContext.Challenges.RemoveRange(DbContext.Challenges);
            DbContext.Tasks.RemoveRange(DbContext.Tasks);
            DbContext.Users.RemoveRange(DbContext.Users);
            DbContext.SaveChanges();

            // Seed users - IDs start at 100 to avoid confusion with counts
            var users = new[]
            {
                new UserProfile 
                { 
                    Id = 100, 
                    Name = "TestUser1", 
                    Email = "test1@test.com", 
                    IsRegistered = true, 
                    PasswordHash = "hash1" 
                },
                new UserProfile 
                { 
                    Id = 101, 
                    Name = "TestUser2", 
                    Email = "test2@test.com", 
                    IsRegistered = true, 
                    PasswordHash = "hash2" 
                },
                new UserProfile 
                { 
                    Id = 102, 
                    Name = "TestUser3", 
                    Email = "test3@test.com", 
                    IsRegistered = false  // Unregistered user for testing
                }
            };

            // Seed tasks - IDs start at 200
            var tasks = new[]
            {
                new HuntTask { Id = 200, Description = "Test Task 1", AuthorId = 100 },
                new HuntTask { Id = 201, Description = "Test Task 2", AuthorId = 101 }
            };

            // Seed challenges - IDs start at 300
            var challenges = new[]
            {
                new Challenge 
                { 
                    Id = 300, 
                    Name = "Test Challenge 1", 
                    TaskId = 200, 
                    JoinCode = "TEST01", 
                    CreatorId = 100, 
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 
                    IsPrivate = false,
                    Status = ChallengeStatus.Open
                },
                new Challenge 
                { 
                    Id = 301, 
                    Name = "Test Challenge 2", 
                    TaskId = 201, 
                    JoinCode = "TEST02", 
                    CreatorId = 101, 
                    CreatedAt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc), 
                    IsPrivate = true,
                    Status = ChallengeStatus.Open
                }
            };

            // Seed photos - IDs start at 400
            var photos = new[]
            {
                new PhotoSubmission 
                { 
                    Id = 400, 
                    TaskId = 200, 
                    UserId = 100, 
                    PhotoUrl = "/test/photo1.jpg", 
                    Votes = 5 
                },
                new PhotoSubmission 
                { 
                    Id = 401, 
                    TaskId = 200, 
                    UserId = 101, 
                    PhotoUrl = "/test/photo2.jpg", 
                    Votes = 3 
                }
            };

            // Seed challenge participants - IDs start at 500
            var challengeParticipants = new[]
            {
                new ChallengeParticipant 
                { 
                    Id = 500, 
                    ChallengeId = 300, 
                    UserId = 100, 
                    Role = ChallengeRole.Admin, 
                    JoinedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) 
                },
                new ChallengeParticipant 
                { 
                    Id = 501, 
                    ChallengeId = 301, 
                    UserId = 101, 
                    Role = ChallengeRole.Admin, 
                    JoinedAt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc) 
                }
            };

            // Add in correct order (respecting foreign keys)
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