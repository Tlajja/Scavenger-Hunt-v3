using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Features.Tasks;
using PhotoScavengerHunt.Features.Hubs;
using PhotoScavengerHunt.Features.Photos;
using Moq;

namespace PhotoScavengerHunt.Tests.Infrastructure
{
    /// <summary>
    /// Base class for tests that provides in-memory database setup
    /// </summary>
    public abstract class DatabaseTestBase : IDisposable
    {
        protected PhotoScavengerHuntDbContext DbContext { get; private set; }
        
        protected DatabaseTestBase()
        {
            // Using a unique database name for each test to ensure isolation
            var options = new DbContextOptionsBuilder<PhotoScavengerHuntDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            DbContext = new PhotoScavengerHuntDbContext(options);
            
        }


        // Seed the database with predictable test data
        protected void SeedTestData()
        {
            // Clear any existing data first
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
                new HuntTask { Id = 200, Description = "Test Task 1", Deadline = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc), Status = HuntTaskStatus.Open, AuthorId = 100 },
                new HuntTask { Id = 201, Description = "Test Task 2", Deadline = new DateTime(2026, 2, 1, 12, 0, 0, DateTimeKind.Utc), Status = HuntTaskStatus.Open, AuthorId = 101 }
            };

            DbContext.Users.AddRange(users);
            DbContext.SaveChanges();
            
            DbContext.Tasks.AddRange(tasks);
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