using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Features.Tasks;
using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Features.Leaderboard;
using Xunit;

namespace PhotoScavengerHunt.Tests.Models
{
    
    public class HuntTaskFactoryTests
    {
        [Fact]
        public void Create_NullDescription_ThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => HuntTaskFactory.Create(null!, 1)
            );
            Assert.Contains("description", exception.Message.ToLower());
        }

        [Fact]
        public void Create_EmptyDescription_ThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => HuntTaskFactory.Create("", 1)
            );
            Assert.Contains("description", exception.Message.ToLower());
        }

        [Fact]
        public void Create_WhitespaceDescription_ThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => HuntTaskFactory.Create("   ", 1)
            );
            Assert.Contains("description", exception.Message.ToLower());
        }

        [Fact]
        public void Create_ValidInput_SetsPropertiesCorrectly()
        {
            var task = HuntTaskFactory.Create("Test Task", 123);
            
            Assert.Equal("Test Task", task.Description);
            Assert.Equal(123, task.AuthorId);
        }
    }

    public class ChallengeFactoryTests
    {
        [Fact]
        public void Create_NullName_ThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => ChallengeFactory.Create(null!, creatorId: 1, taskIds: new[] { 1 })
            );
            Assert.Contains("name", exception.Message.ToLower());
        }

        [Fact]
        public void Create_EmptyName_ThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => ChallengeFactory.Create("", creatorId: 1, taskIds: new[] { 1 })
            );
            Assert.Contains("name", exception.Message.ToLower());
        }

        [Fact]
        public void Create_WhitespaceName_ThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => ChallengeFactory.Create("   ", creatorId: 1, taskIds: new[] { 1 })
            );
            Assert.Contains("name", exception.Message.ToLower());
        }

        [Fact]
        public void Create_ValidInput_SetsDefaultValues()
        {
            var challenge = ChallengeFactory.Create("Test", creatorId: 2, taskIds: new[] { 1 });
            
            Assert.Equal("Test", challenge.Name);
            Assert.Contains(challenge.ChallengeTasks, ct => ct.TaskId == 1);
            Assert.Equal(2, challenge.CreatorId);
            Assert.Equal(ChallengeStatus.Open, challenge.Status);
            Assert.NotEqual(default(DateTime), challenge.CreatedAt);
        }

        [Fact]
        public void Create_WithDeadline_UsesProvidedDeadline()
        {
            var deadline = DateTime.UtcNow.AddDays(3);
            var challenge = ChallengeFactory.Create("Test", creatorId: 2, taskIds: new[] { 1 }, deadline: deadline);
            
            Assert.Equal(deadline, challenge.Deadline);
        }

        [Fact]
        public void Create_WithoutDeadline_SetsDefault7Days()
        {
            var beforeCreate = DateTime.UtcNow.AddDays(6);
            var challenge = ChallengeFactory.Create("Test", creatorId: 2, taskIds: new[] { 1 });
            var afterCreate = DateTime.UtcNow.AddDays(8);
            
            Assert.NotNull(challenge.Deadline);
            Assert.True(challenge.Deadline >= beforeCreate);
            Assert.True(challenge.Deadline <= afterCreate);
        }
    }

    public class ValidationExtensionsTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void IsValidUsername_NullOrWhitespace_ReturnsFalse(string? username)
        {
            Assert.False(ValidationExtensions.IsValidUsername(username!));
        }

        [Theory]
        [InlineData("a")]
        [InlineData("ABCDEFGHIJ01234567890")]
        public void IsValidUsername_InvalidLength_ReturnsFalse(string username)
        {
            Assert.False(ValidationExtensions.IsValidUsername(username));
        }

        [Theory]
        [InlineData("user name")]
        [InlineData("user-name")]
        [InlineData("user_name")]
        [InlineData("user@name")]
        [InlineData("user.name")]
        public void IsValidUsername_InvalidCharacters_ReturnsFalse(string username)
        {
            Assert.False(ValidationExtensions.IsValidUsername(username));
        }

        [Theory]
        [InlineData("ab")]
        [InlineData("ABCDEFGHIJ0123456789")]
        [InlineData("User123")]
        [InlineData("ABC")]
        public void IsValidUsername_ValidInput_ReturnsTrue(string username)
        {
            Assert.True(ValidationExtensions.IsValidUsername(username));
        }
    }

    public class LeaderboardEntryTests
    {
        [Fact]
        public void CompareTo_DifferentVotes_SortsByVotesDescending()
        {
            var entry1 = new LeaderboardEntry(1, "User1", 10);
            var entry2 = new LeaderboardEntry(2, "User2", 20);

            var result = entry1.CompareTo(entry2);

            Assert.True(result > 0); // entry1 should come after entry2
        }

        [Fact]
        public void CompareTo_SameVotes_SortsByNameAscending()
        {
            var entry1 = new LeaderboardEntry(1, "Bob", 10);
            var entry2 = new LeaderboardEntry(2, "Alice", 10);

            var result = entry1.CompareTo(entry2);

            Assert.True(result > 0); // Bob comes after Alice
        }

        [Fact]
        public void CompareTo_SameVotesAndName_SortsByUserIdAscending()
        {
            var entry1 = new LeaderboardEntry(2, "Alice", 10);
            var entry2 = new LeaderboardEntry(1, "Alice", 10);

            var result = entry1.CompareTo(entry2);

            Assert.True(result > 0); // UserId 2 comes after UserId 1
        }

        [Fact]
        public void CompareTo_SortingMultipleEntries_WorksCorrectly()
        {
            var entries = new List<LeaderboardEntry>
            {
                new LeaderboardEntry(1, "Alice", 10),
                new LeaderboardEntry(2, "Bob", 15),
                new LeaderboardEntry(3, "Charlie", 10),
                new LeaderboardEntry(4, "David", 20)
            };

            entries.Sort();

            Assert.Equal(4, entries[0].UserId); // David (20 votes)
            Assert.Equal(2, entries[1].UserId); // Bob (15 votes)
            Assert.Equal(1, entries[2].UserId); // Alice (10, before Charlie)
            Assert.Equal(3, entries[3].UserId); // Charlie (10)
        }
    }

    // Test record types for proper immutability
    public class RequestModelTests
    {
        [Fact]
        public void JoinChallengeRequest_Deconstruction_WorksCorrectly()
        {
            var request = new JoinChallengeRequest("ABC123", 42);
            
            var (code, userId) = request;
            
            Assert.Equal("ABC123", code);
            Assert.Equal(42, userId);
        }

        [Fact]
        public void RegisterRequest_AllProperties_SetCorrectly()
        {
            var request = new RegisterRequest("test@test.com", "pass123", "User");
            
            Assert.Equal("test@test.com", request.Email);
            Assert.Equal("pass123", request.Password);
            Assert.Equal("User", request.Username);
        }
    }

    // Test enum values
    public class EnumTests
    {
        [Fact]
        public void ChallengeRole_HasCorrectValues()
        {
            Assert.Equal(0, (int)ChallengeRole.Participant);
            Assert.Equal(1, (int)ChallengeRole.Admin);
        }

        [Fact]
        public void ChallengeStatus_HasCorrectValues()
        {
            Assert.Equal(0, (int)ChallengeStatus.Open);
            Assert.Equal(1, (int)ChallengeStatus.Closed);
            Assert.Equal(2, (int)ChallengeStatus.Completed);
        }
    }

    // Test default initialization (this IS meaningful - tests that defaults are safe)
    public class DefaultInitializationTests
    {
        [Fact]
        public void PhotoSubmission_DefaultComments_IsEmptyNotNull()
        {
            // This tests that we won't get NullReferenceException when accessing Comments
            var submission = new PhotoSubmission();
            
            Assert.NotNull(submission.Comments);
            Assert.Empty(submission.Comments);
            // Can safely add items without null check
            submission.Comments.Add(new Comment());
        }

        [Fact]
        public void UserProfile_DefaultIsRegistered_IsFalse()
        {
            // Tests that new users aren't accidentally marked as registered
            var user = new UserProfile();
            
            Assert.False(user.IsRegistered);
        }

        [Fact]
        public void Challenge_DefaultStatus_IsOpen()
        {
            // Tests that challenges start in correct state
            var challenge = new Challenge();
            
            Assert.Equal(ChallengeStatus.Open, challenge.Status);
        }

        [Fact]
        public void ChallengeParticipant_DefaultRole_IsParticipant()
        {
            // Tests that users aren't accidentally made admins
            var participant = new ChallengeParticipant();
            
            Assert.Equal(ChallengeRole.Participant, participant.Role);
        }
    }
}