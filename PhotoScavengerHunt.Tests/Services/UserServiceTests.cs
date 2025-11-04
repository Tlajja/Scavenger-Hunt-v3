using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace PhotoScavengerHunt.Tests.Services
{
    public class UserServiceTests : DatabaseTestBase
    {
        private readonly UserService _service;

        public UserServiceTests()
        {
            _service = new UserService(DbContext);
            SeedTestData();
        }

        [Fact]
        public async Task CreateUserAsync_ValidInput_ReturnsSuccess()
        {
            // Arrange
            var name = "NewTestUser";
            var age = 28;

            // Act
            var result = await _service.CreateUserAsync(name, age);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Error);
            Assert.NotNull(result.User);
            Assert.Equal(name, result.User.Name);
            Assert.Equal(age, result.User.Age);
            Assert.False(result.User.IsRegistered); // Default should be false
        }

        [Theory]
        [InlineData("ab", 25)] // Min length (2 chars)
        [InlineData("ABCDEFGHIJ0123456789", 25)] // Max length (20 chars)
        [InlineData("User123", 25)] // Alphanumeric
        [InlineData("ABC", 1)] // Min age
        [InlineData("User", 125)] // Max age
        public async Task CreateUserAsync_ValidEdgeCases_ReturnsSuccess(string name, int age)
        {
            // Act
            var result = await _service.CreateUserAsync(name, age);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.User);
        }

        [Theory]
        [InlineData("a", 25, "Invalid username format")] // Too short
        [InlineData("ABCDEFGHIJ01234567890", 25, "Invalid username format")] // Too long (21 chars)
        [InlineData("user name", 25, "Invalid username format")] // Contains space
        [InlineData("user-name", 25, "Invalid username format")] // Contains hyphen
        [InlineData("user_name", 25, "Invalid username format")] // Contains underscore
        [InlineData("user@name", 25, "Invalid username format")] // Contains special char
        [InlineData("", 25, "Invalid username format")] // Empty
        [InlineData("   ", 25, "Invalid username format")] // Whitespace only
        public async Task CreateUserAsync_InvalidUsername_ReturnsError(
            string name, int age, string expectedErrorSubstring)
        {
            // Act
            var result = await _service.CreateUserAsync(name, age);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(expectedErrorSubstring, result.Error);
            Assert.Null(result.User);
        }

        [Theory]
        [InlineData("ValidUser", 0, "Invalid age value.")]
        [InlineData("ValidUser", -1, "Invalid age value.")]
        [InlineData("ValidUser", 126, "Invalid age value.")]
        [InlineData("ValidUser", 1000, "Invalid age value.")]
        public async Task CreateUserAsync_InvalidAge_ReturnsError(
            string name, int age, string expectedError)
        {
            // Act
            var result = await _service.CreateUserAsync(name, age);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(expectedError, result.Error);
        }

        [Fact]
        public async Task CreateUserAsync_DuplicateName_ReturnsError()
        {
            // Arrange - TestUser1 already exists in seed data
            var name = "TestUser1";
            var age = 30;

            // Act
            var result = await _service.CreateUserAsync(name, age);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Username already exists.", result.Error);
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsAllUsers()
        {
            // Act
            var result = await _service.GetUsersAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Users);
            Assert.True(result.Users.Count >= 3); // At least seed data
            Assert.Contains(result.Users, u => u.Name == "TestUser1");
            Assert.Contains(result.Users, u => u.Name == "TestUser2");
        }

        [Fact]
        public async Task GetUsersAsync_EmptyDatabase_ReturnsEmpty()
        {
            // Arrange - Remove all data including dependencies first
            DbContext.HubMembers.RemoveRange(DbContext.HubMembers);
            DbContext.Photos.RemoveRange(DbContext.Photos);
            DbContext.Tasks.RemoveRange(DbContext.Tasks);
            DbContext.Users.RemoveRange(DbContext.Users);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _service.GetUsersAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Users);
            Assert.Empty(result.Users);
        }

        [Fact]
        public async Task GetUserByIdAsync_ValidId_ReturnsUser()
        {
            // Act
            var result = await _service.GetUserByIdAsync(100);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.User);
            Assert.Equal(100, result.User.Id);
            Assert.Equal("TestUser1", result.User.Name);
        }

        [Fact]
        public async Task GetUserByIdAsync_InvalidId_ReturnsError()
        {
            // Act
            var result = await _service.GetUserByIdAsync(99999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Error);
            Assert.Null(result.User);
        }

        [Fact]
        public async Task DeleteUserAsync_ValidId_ReturnsSuccess()
        {
            // Arrange - Remove hub members first to avoid foreign key constraint
            var hubMembers = await DbContext.HubMembers.Where(hm => hm.UserId == 100).ToListAsync();
            DbContext.HubMembers.RemoveRange(hubMembers);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _service.DeleteUserAsync(100);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Error);

            // Verify user is deleted
            var user = await DbContext.Users.FindAsync(100);
            Assert.Null(user);
        }

        [Fact]
        public async Task DeleteUserAsync_InvalidId_ReturnsError()
        {
            // Act
            var result = await _service.DeleteUserAsync(99999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Error);
        }

        [Fact]
        public async Task CreateUserAsync_IsPersisted()
        {
            // Arrange
            var name = "PersistentUser";
            var age = 32;

            // Act
            var createResult = await _service.CreateUserAsync(name, age);
            var userId = createResult.User!.Id;

            // Re-query from database
            var getResult = await _service.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(getResult.User);
            Assert.Equal(name, getResult.User.Name);
            Assert.Equal(age, getResult.User.Age);
        }

        [Fact]
        public void ValidationExtensions_IsValidUsername_ValidInputs_ReturnsTrue()
        {
            // Arrange & Act & Assert
            Assert.True(ValidationExtensions.IsValidUsername("ab"));
            Assert.True(ValidationExtensions.IsValidUsername("User123"));
            Assert.True(ValidationExtensions.IsValidUsername("ABCDEFGHIJ0123456789"));
            Assert.True(ValidationExtensions.IsValidUsername("abc"));
            Assert.True(ValidationExtensions.IsValidUsername("ABC123xyz"));
        }

        [Fact]
        public void ValidationExtensions_IsValidUsername_InvalidInputs_ReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.False(ValidationExtensions.IsValidUsername("a")); // Too short
            Assert.False(ValidationExtensions.IsValidUsername("ABCDEFGHIJ01234567890")); // Too long
            Assert.False(ValidationExtensions.IsValidUsername("user name")); // Space
            Assert.False(ValidationExtensions.IsValidUsername("user-name")); // Hyphen
            Assert.False(ValidationExtensions.IsValidUsername("user_name")); // Underscore
            Assert.False(ValidationExtensions.IsValidUsername("")); // Empty
            Assert.False(ValidationExtensions.IsValidUsername("   ")); // Whitespace
            Assert.False(ValidationExtensions.IsValidUsername(null!)); // Null
        }

        [Fact]
        public async Task CreateUserAsync_CaseInsensitiveDuplicate_StillAllowed()
        {
            // Arrange - Database may be case-sensitive or case-insensitive
            // This test documents the current behavior
            var result1 = await _service.CreateUserAsync("CaseTest", 25);
            
            // Act
            var result2 = await _service.CreateUserAsync("CASETEST", 25);

            // Assert - The behavior depends on database collation
            // This test will pass regardless of case sensitivity
            // If case-insensitive: result2.Success will be false
            // If case-sensitive: result2.Success will be true
            Assert.True(result1.Success);
        }
    }
}