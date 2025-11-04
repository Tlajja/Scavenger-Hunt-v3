using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Features.Tasks;
using PhotoScavengerHunt.Features.Hubs;
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
            // Act
            var user = new UserProfile();

            // Assert
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
            // Arrange
            var user = new UserProfile();

            // Act
            user.Id = 1;
            user.Email = "test@test.com";
            user.PasswordHash = "hash";
            user.Name = "TestUser";
            user.Age = 25;
            user.IsRegistered = true;

            // Assert
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
            // Act
            var task = new HuntTask();

            // Assert
            Assert.Equal(0, task.Id);
            Assert.Equal("", task.Description);
            Assert.Equal(default(DateTime), task.Deadline);
            Assert.Equal(HuntTaskStatus.Open, task.Status);
            Assert.Equal(0, task.AuthorId);
        }

        [Fact]
        public void HuntTaskStatus_HasCorrectValues()
        {
            // Assert
            Assert.Equal(0, (int)HuntTaskStatus.Open);
            Assert.Equal(1, (int)HuntTaskStatus.Closed);
            Assert.Equal(2, (int)HuntTaskStatus.Completed);
        }
    }

}