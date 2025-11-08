using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Features.Tasks;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Features.Leaderboard;
using Xunit;
using System.Text.Json;

namespace PhotoScavengerHunt.Tests.Models
{
    public class UserProfileTests
    {
        [Fact]
        public void UserProfile_DefaultValues_AreCorrect()
        {
            var user = new UserProfile();

            Assert.Equal(0, user.Id);
            Assert.Equal("", user.Email);
            Assert.Equal("", user.PasswordHash);
            Assert.Equal("", user.Name);
            Assert.Equal(0, user.Age);
            Assert.False(user.IsRegistered);
        }

        [Fact]
        public void UserProfile_SetProperties_WorksCorrectly()
        {
            var user = new UserProfile();

            user.Id = 1;
            user.Email = "test@test.com";
            user.PasswordHash = "hash";
            user.Name = "TestUser";
            user.Age = 25;
            user.IsRegistered = true;

            Assert.Equal(1, user.Id);
            Assert.Equal("test@test.com", user.Email);
            Assert.Equal("hash", user.PasswordHash);
            Assert.Equal("TestUser", user.Name);
            Assert.Equal(25, user.Age);
            Assert.True(user.IsRegistered);
        }
    }

    public class HuntTaskTests
    {
        [Fact]
        public void HuntTask_DefaultValues_AreCorrect()
        {
            var task = new HuntTask();

            Assert.Equal(0, task.Id);
            Assert.Equal("", task.Description);
            Assert.Equal(default(DateTime), task.Deadline);
            Assert.Equal(HuntTaskStatus.Open, task.Status);
            Assert.Equal(0, task.AuthorId);
        }

        [Fact]
        public void HuntTaskStatus_HasCorrectValues()
        {
            Assert.Equal(0, (int)HuntTaskStatus.Open);
            Assert.Equal(1, (int)HuntTaskStatus.Closed);
            Assert.Equal(2, (int)HuntTaskStatus.Completed);
        }
    }

    public class PhotoSubmissionTests
    {
        [Fact]
        public void PhotoSubmission_DefaultValues_AreCorrect()
        {
            var photo = new PhotoSubmission();

            Assert.Equal(0, photo.Id);
            Assert.Equal(0, photo.TaskId);
            Assert.Equal(0, photo.UserId);
            Assert.Null(photo.HubId);
            Assert.Equal(string.Empty, photo.PhotoUrl);
            Assert.Equal(0, photo.Votes);
            Assert.NotNull(photo.Comments);
            Assert.Empty(photo.Comments);
        }

        [Fact]
        public void PhotoSubmission_HubId_CanBeNull()
        {
            var photo = new PhotoSubmission { HubId = null };

            Assert.Null(photo.HubId);
        }

        [Fact]
        public void PhotoSubmission_HubId_CanBeSet()
        {
            var photo = new PhotoSubmission { HubId = 5 };

            Assert.Equal(5, photo.HubId);
        }
    }

    public class CommentTests
    {
        [Fact]
        public void Comment_DefaultValues_AreCorrect()
        {
            var comment = new Comment();

            Assert.Equal(0, comment.Id);
            Assert.Equal(0, comment.UserId);
            Assert.Equal("", comment.Text);
            Assert.Equal(default(DateTime), comment.Timestamp);
            Assert.Equal(0, comment.PhotoSubmissionId);
            Assert.Null(comment.PhotoSubmission);
        }

        [Fact]
        public void Comment_JsonSerialization_ExcludesPhotoSubmission()
        {
            var comment = new Comment
            {
                Id = 1,
                UserId = 1,
                Text = "Test comment",
                Timestamp = DateTime.UtcNow,
                PhotoSubmissionId = 1,
                PhotoSubmission = new PhotoSubmission()
            };

            var json = JsonSerializer.Serialize(comment);

            Assert.DoesNotContain("\"photoSubmission\"", json.ToLower());
        }
    }

    public class LeaderboardEntryTests
    {
        [Fact]
        public void LeaderboardEntry_Constructor_SetsProperties()
        {
            var entry = new LeaderboardEntry(1, "TestUser", 10);

            Assert.Equal(1, entry.UserId);
            Assert.Equal("TestUser", entry.UserName);
            Assert.Equal(10, entry.TotalVotes);
        }

        [Fact]
        public void LeaderboardEntry_CompareTo_SortsByVotesDescending()
        {
            var entry1 = new LeaderboardEntry(1, "User1", 10);
            var entry2 = new LeaderboardEntry(2, "User2", 20);

            var result = entry1.CompareTo(entry2);

            Assert.True(result > 0); // entry1 should come after entry2 (20 > 10)
        }

        [Fact]
        public void LeaderboardEntry_CompareTo_SameVotes_SortsByNameAscending()
        {
            var entry1 = new LeaderboardEntry(1, "Bob", 10);
            var entry2 = new LeaderboardEntry(2, "Alice", 10);

            var result = entry1.CompareTo(entry2);

            Assert.True(result > 0); // Bob should come after Alice
        }

        [Fact]
        public void LeaderboardEntry_CompareTo_SameVotesAndName_SortsByUserIdAscending()
        {
            var entry1 = new LeaderboardEntry(2, "Alice", 10);
            var entry2 = new LeaderboardEntry(1, "Alice", 10);

            var result = entry1.CompareTo(entry2);

            Assert.True(result > 0); // UserId 2 should come after UserId 1
        }
    }

    public class RequestModelTests
    {
        [Fact]
        public void CreateTaskRequest_Record_WorksCorrectly()
        {
            var deadline = new DateTime(2026, 1, 1);

            var request = new CreateTaskRequest("Task description", deadline, 1);

            Assert.Equal("Task description", request.Description);
            Assert.Equal(deadline, request.Deadline);
            Assert.Equal(1, request.AuthorId);
        }

        [Fact]
        public void AddCommentRequest_Record_WorksCorrectly()
        {
            var request = new AddCommentRequest(1, "Comment text");

            Assert.Equal(1, request.UserId);
            Assert.Equal("Comment text", request.Text);
        }

        [Fact]
        public void RegisterRequest_Record_WorksCorrectly()
        {
            var request = new RegisterRequest(
                "test@test.com", 
                "password123", 
                "TestUser", 
                25
            );

            Assert.Equal("test@test.com", request.Email);
            Assert.Equal("password123", request.Password);
            Assert.Equal("TestUser", request.Username);
            Assert.Equal(25, request.Age);
        }

        [Fact]
        public void LoginRequest_Record_WorksCorrectly()
        {
            var request = new LoginRequest("TestUser", "password123");

            Assert.Equal("TestUser", request.Username);
            Assert.Equal("password123", request.Password);
        }
    }

    public class ValidationExtensionsTests
    {
        [Theory]
        [InlineData("ab", true)]
        [InlineData("ABCDEFGHIJ0123456789", true)]
        [InlineData("User123", true)]
        [InlineData("a", false)]
        [InlineData("ABCDEFGHIJ01234567890", false)]
        [InlineData("user name", false)]
        [InlineData("", false)]
        public void IsValidUsername_VariousInputs_ReturnsExpectedResult(
            string? username, bool expected)
        {
            var result = ValidationExtensions.IsValidUsername(username!);

            Assert.Equal(expected, result);
        }
        
        [Fact]
        public void IsValidUsername_NullInput_ReturnsFalse()
        {
            var result = ValidationExtensions.IsValidUsername(null!);

            Assert.False(result);
        }
    }
}