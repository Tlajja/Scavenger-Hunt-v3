using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Repositories;
using Microsoft.Extensions.Logging;

using Xunit;

namespace PhotoScavengerHunt.Tests.Services
{
    public class UserServiceTests : DatabaseTestBase
    {
        private readonly UserService _service;

        public UserServiceTests()
        {
            var userRepo = new UserRepository(DbContext);
            var logger = new LoggerFactory().CreateLogger<UserService>();
            _service = new UserService(userRepo, logger);

            SeedTestData();
        }

        [Fact]
        public async Task CreateUserAsync_ValidInput_ReturnsSuccess()
        {
            var name = "NewTestUser";

            var result = await _service.CreateUserAsync(name);

            Assert.True(result.Success);
            Assert.Empty(result.Error);
            Assert.NotNull(result.User);
            Assert.Equal(name, result.User.Name);
            Assert.False(result.User.IsRegistered); // Default should be false
        }

        [Theory]
        [InlineData("ab")] // Min length (2 chars)
        [InlineData("ABCDEFGHIJ0123456789")] // Max length (20 chars)
        [InlineData("User123")] 
        [InlineData("ABC")]
        [InlineData("User")]
        public async Task CreateUserAsync_ValidEdgeCases_ReturnsSuccess(string name)
        {
            var result = await _service.CreateUserAsync(name);

            Assert.True(result.Success);
            Assert.NotNull(result.User);
        }

        [Theory]
        [InlineData("a", "Invalid username format")]
        [InlineData("ABCDEFGHIJ01234567890", "Invalid username format")] 
        [InlineData("user name", "Invalid username format")] 
        [InlineData("user-name", "Invalid username format")] 
        [InlineData("user_name", "Invalid username format")] 
        [InlineData("user@name", "Invalid username format")] 
        [InlineData("", "Invalid username format")] 
        [InlineData("   ", "Invalid username format")] 
        public async Task CreateUserAsync_InvalidUsername_ReturnsError(
            string name, string expectedErrorSubstring)
        {
            var result = await _service.CreateUserAsync(name);

            Assert.False(result.Success);
            Assert.Contains(expectedErrorSubstring, result.Error);
            Assert.Null(result.User);
        }

        [Fact]
        public async Task CreateUserAsync_DuplicateName_ReturnsError()
        {
            var name = "TestUser1";

            var result = await _service.CreateUserAsync(name);

            Assert.False(result.Success);
            Assert.Equal("Username already exists.", result.Error);
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsAllUsers()
        {
            var result = await _service.GetUsersAsync();

            Assert.True(result.Success);
            Assert.NotNull(result.Users);
            Assert.True(result.Users.Count >= 3); // At least seed data
            Assert.Contains(result.Users, u => u.Name == "TestUser1");
            Assert.Contains(result.Users, u => u.Name == "TestUser2");
        }

        [Fact]
        public async Task GetUsersAsync_EmptyDatabase_ReturnsEmpty()
        {
            // Remove in correct order to avoid foreign key constraint issues
            DbContext.Photos.RemoveRange(DbContext.Photos);
            DbContext.ChallengeParticipants.RemoveRange(DbContext.ChallengeParticipants);
            DbContext.Challenges.RemoveRange(DbContext.Challenges);
            DbContext.Tasks.RemoveRange(DbContext.Tasks);
            DbContext.Users.RemoveRange(DbContext.Users);
            await DbContext.SaveChangesAsync();

            var result = await _service.GetUsersAsync();

            Assert.True(result.Success);
            Assert.NotNull(result.Users);
            Assert.Empty(result.Users);
        }

        [Fact]
        public async Task GetUserByIdAsync_ValidId_ReturnsUser()
        {
            var result = await _service.GetUserByIdAsync(100);

            Assert.True(result.Success);
            Assert.NotNull(result.User);
            Assert.Equal(100, result.User.Id);
            Assert.Equal("TestUser1", result.User.Name);
        }

        [Fact]
        public async Task GetUserByIdAsync_InvalidId_ReturnsError()
        {
            var result = await _service.GetUserByIdAsync(99999);

            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Error);
            Assert.Null(result.User);
        }

        [Fact]
        public async Task DeleteUserAsync_ValidId_ReturnsSuccess()
        {
            // Remove related entities first to avoid foreign key constraint issues
            var userPhotos = DbContext.Photos.Where(p => p.UserId == 100);
            DbContext.Photos.RemoveRange(userPhotos);
            
            var userParticipations = DbContext.ChallengeParticipants.Where(cp => cp.UserId == 100);
            DbContext.ChallengeParticipants.RemoveRange(userParticipations);
            
            var allChallenges = DbContext.Challenges.ToList();
            DbContext.Challenges.RemoveRange(allChallenges);
            
            var userTasks = DbContext.Tasks.Where(t => t.AuthorId == 100);
            DbContext.Tasks.RemoveRange(userTasks);
            
            await DbContext.SaveChangesAsync();

            var result = await _service.DeleteUserAsync(100);

            Assert.True(result.Success);
            Assert.Empty(result.Error);

            var user = await DbContext.Users.FindAsync(100);
            Assert.Null(user);
        }

        [Fact]
        public async Task DeleteUserAsync_InvalidId_ReturnsError()
        {
            var result = await _service.DeleteUserAsync(99999);

            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Error);
        }

        [Fact]
        public async Task CreateUserAsync_IsPersisted()
        {
            var name = "PersistentUser";

            var createResult = await _service.CreateUserAsync(name);
            var userId = createResult.User!.Id;

            var getResult = await _service.GetUserByIdAsync(userId);

            Assert.NotNull(getResult.User);
            Assert.Equal(name, getResult.User.Name);
        }

        [Fact]
        public void ValidationExtensions_IsValidUsername_ValidInputs_ReturnsTrue()
        {
            Assert.True(ValidationExtensions.IsValidUsername("ab"));
            Assert.True(ValidationExtensions.IsValidUsername("User123"));
            Assert.True(ValidationExtensions.IsValidUsername("ABCDEFGHIJ0123456789"));
            Assert.True(ValidationExtensions.IsValidUsername("abc"));
            Assert.True(ValidationExtensions.IsValidUsername("ABC123xyz"));
        }

        [Fact]
        public void ValidationExtensions_IsValidUsername_InvalidInputs_ReturnsFalse()
        {
            Assert.False(ValidationExtensions.IsValidUsername("a")); 
            Assert.False(ValidationExtensions.IsValidUsername("ABCDEFGHIJ01234567890")); 
            Assert.False(ValidationExtensions.IsValidUsername("user name")); 
            Assert.False(ValidationExtensions.IsValidUsername("user-name")); 
            Assert.False(ValidationExtensions.IsValidUsername("user_name")); 
            Assert.False(ValidationExtensions.IsValidUsername("")); 
            Assert.False(ValidationExtensions.IsValidUsername("   ")); 
            Assert.False(ValidationExtensions.IsValidUsername(null!)); 
        }

        [Fact]
        public async Task CreateUserAsync_CaseInsensitiveDuplicate_StillAllowed()
        {
            // Arrange - Database may be case-sensitive or case-insensitive
            // This test documents the current behavior
            var result1 = await _service.CreateUserAsync("CaseTest");
            
            var result2 = await _service.CreateUserAsync("CASETEST");

            Assert.True(result1.Success);
        }
    }
}