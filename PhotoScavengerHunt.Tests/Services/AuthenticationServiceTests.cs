using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Repositories;

using Xunit;

namespace PhotoScavengerHunt.Tests.Services
{
    public class AuthenticationServiceTests : DatabaseTestBase
    {
        private readonly AuthenticationService _service;

        public AuthenticationServiceTests()
        {
            var userRepo = new UserRepository(DbContext);
            _service = new AuthenticationService(userRepo);

            SeedTestData();
        }

        [Fact]
        public async Task RegisterAsync_ValidRequest_ReturnsSuccess()
        {
            var request = new RegisterRequest(
                Email: "newuser@test.com",
                Password: "password123",
                Username: "NewUser",
                Age: 25
            );

            var result = await _service.RegisterAsync(request);

            Assert.True(result.Success);
            Assert.Equal("Registration successful.", result.Message);
            Assert.NotNull(result.Data);
            
            // Use reflection to access anonymous type properties
            var dataType = result.Data.GetType();
            var userIdProp = dataType.GetProperty("userId");
            var userId = (int)userIdProp!.GetValue(result.Data)!;
            
            var user = await DbContext.Users.FindAsync(userId);
            Assert.NotNull(user);
            Assert.Equal("NewUser", user.Name);
            Assert.True(user.IsRegistered);
        }

        [Theory]
        [InlineData("", "password123", "user", 25, "Email, password, and username are required.")]
        [InlineData("test@test.com", "", "user", 25, "Email, password, and username are required.")]
        [InlineData("test@test.com", "password123", "", 25, "Email, password, and username are required.")]
        [InlineData("invalid-email", "password123", "user", 25, "Invalid email format.")]
        [InlineData("test@test.com", "short", "user", 25, "Password must be at least 6 characters long.")]
        [InlineData("test@test.com", "password123", "u", 25, "Invalid username format")]
        [InlineData("test@test.com", "password123", "user with spaces", 25, "Invalid username format")]
        [InlineData("test@test.com", "password123", "user", 0, "Invalid age value.")]
        [InlineData("test@test.com", "password123", "user", 126, "Invalid age value.")]
        public async Task RegisterAsync_InvalidInput_ReturnsError(
            string email, string password, string username, int age, string expectedError)
        {
            var request = new RegisterRequest(email, password, username, age);

            var result = await _service.RegisterAsync(request);

            Assert.False(result.Success);
            Assert.Equal(expectedError, result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ReturnsError()
        {
            var request = new RegisterRequest(
                Email: "test1@test.com", // Already exists in seed data
                Password: "password123",
                Username: "UniqueUser",
                Age: 25
            );

            var result = await _service.RegisterAsync(request);

            Assert.False(result.Success);
            Assert.Equal("Email already registered.", result.Message);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateUsername_ReturnsError()
        {
            var request = new RegisterRequest(
                Email: "unique@test.com",
                Password: "password123",
                Username: "TestUser1", // Already exists in seed data
                Age: 25
            );

            var result = await _service.RegisterAsync(request);

            Assert.False(result.Success);
            Assert.Equal("Username already exists.", result.Message);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
        {
            // First register a user
            var registerRequest = new RegisterRequest(
                Email: "login@test.com",
                Password: "password123",
                Username: "LoginUser",
                Age: 25
            );
            await _service.RegisterAsync(registerRequest);

            var loginRequest = new LoginRequest("LoginUser", "password123");

            var result = await _service.LoginAsync(loginRequest);

            Assert.True(result.Success);
            Assert.Equal("Login successful.", result.Message);
            Assert.NotNull(result.Data);
            
            // Use reflection to access anonymous type properties
            var dataType = result.Data.GetType();
            var usernameProp = dataType.GetProperty("username");
            var username = (string)usernameProp!.GetValue(result.Data)!;
            
            Assert.Equal("LoginUser", username);
        }

        [Theory]
        [InlineData("", "password", "Username and password are required.")]
        [InlineData("user", "", "Username and password are required.")]
        public async Task LoginAsync_MissingCredentials_ReturnsError(
            string username, string password, string expectedError)
        {
            var request = new LoginRequest(username, password);

            var result = await _service.LoginAsync(request);

            Assert.False(result.Success);
            Assert.Equal(expectedError, result.Message);
        }

        [Fact]
        public async Task LoginAsync_NonExistentUser_ReturnsError()
        {
            var request = new LoginRequest("NonExistent", "password123");

            var result = await _service.LoginAsync(request);

            Assert.False(result.Success);
            Assert.Equal("Invalid username or password.", result.Message);
        }

        [Fact]
        public async Task LoginAsync_UnregisteredUser_ReturnsError()
        {
            // TestUser3 is not registered in seed data
            var request = new LoginRequest("TestUser3", "anypassword");

            var result = await _service.LoginAsync(request);

            Assert.False(result.Success);
            Assert.Equal("Please complete registration first.", result.Message);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ReturnsError()
        {
            // Register a user first
            var registerRequest = new RegisterRequest(
                Email: "wrongpass@test.com",
                Password: "correctpass",
                Username: "WrongPassUser",
                Age: 25
            );
            await _service.RegisterAsync(registerRequest);

            var loginRequest = new LoginRequest("WrongPassUser", "wrongpass");

            var result = await _service.LoginAsync(loginRequest);

            Assert.False(result.Success);
            Assert.Equal("Invalid username or password.", result.Message);
        }

        [Fact]
        public async Task PasswordHashing_IsDeterministic()
        {
            var request1 = new RegisterRequest("hash1@test.com", "samepass", "HashUser1", 25);
            var request2 = new RegisterRequest("hash2@test.com", "samepass", "HashUser2", 25);

            await _service.RegisterAsync(request1);
            await _service.RegisterAsync(request2);

            var user1 = await DbContext.Users.FirstAsync(u => u.Name == "HashUser1");
            var user2 = await DbContext.Users.FirstAsync(u => u.Name == "HashUser2");

            // Same password should produce same hash
            Assert.Equal(user1.PasswordHash, user2.PasswordHash);
        }
    }
}