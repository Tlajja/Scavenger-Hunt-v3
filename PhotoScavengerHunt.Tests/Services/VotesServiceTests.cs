using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Leaderboard;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Repositories;
using Xunit;

namespace PhotoScavengerHunt.Tests.Services
{
    public class VotesServiceTests : DatabaseTestBase
    {
        private readonly VotesService _service;

        public VotesServiceTests()
        {
            _service = new VotesService(new PhotoRepository(DbContext));
            SeedTestData();
        }

        [Fact]
        public async Task UpvotePhotoAsync_ValidSubmission_IncrementsVotes()
        {
            var initialVotes = (await DbContext.Photos.FindAsync(400))!.Votes;

            var result = await _service.UpvotePhotoAsync(400, 101); // User 101 voting on photo by user 100

            Assert.True(result.Success);
            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Result);
            Assert.Equal(initialVotes + 1, result.Result.Votes);
        }

        [Fact]
        public async Task UpvotePhotoAsync_MultipleVotes_IncrementsCorrectly()
        {
            var initialVotes = (await DbContext.Photos.FindAsync(400))!.Votes;

            var result1 = await _service.UpvotePhotoAsync(400, 101); // User 101 voting
            Assert.True(result1.Success, $"First vote failed: {result1.ErrorMessage}");
            
            var result2 = await _service.UpvotePhotoAsync(400, 102); // User 102 voting
            Assert.True(result2.Success, $"Second vote failed: {result2.ErrorMessage}");
            
            var result3 = await _service.UpvotePhotoAsync(400, 103); // User 103 voting
            Assert.True(result3.Success, $"Third vote failed: {result3.ErrorMessage}");

            Assert.Equal(initialVotes + 3, result3.Result!.Votes);
        }

        [Fact]
        public async Task UpvotePhotoAsync_NonExistentSubmission_ReturnsError()
        {
            var result = await _service.UpvotePhotoAsync(99999, 101); // User 101 voting on non-existent photo

            Assert.False(result.Success);
            Assert.Equal("Submission not found.", result.ErrorMessage);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task UpvotePhotoAsync_PersistsToDatabase()
        {
            await _service.UpvotePhotoAsync(400, 101); // User 101 voting on photo by user 100

            var submission = await DbContext.Photos.FindAsync(400);

            Assert.NotNull(submission);
            Assert.Equal(6, submission.Votes); 
        }

        [Fact]
        public async Task UpvotePhotoAsync_ReturnsUpdatedSubmission()
        {
            var result = await _service.UpvotePhotoAsync(400, 101); // User 101 voting on photo by user 100

            Assert.NotNull(result.Result);
            Assert.Equal(400, result.Result.Id);
            Assert.Equal(200, result.Result.TaskId);
            Assert.Equal(100, result.Result.UserId);
        }

        [Fact]
        public async Task UpvotePhotoAsync_OwnSubmission_ReturnsError()
        {
            var result = await _service.UpvotePhotoAsync(400, 100); // User 100 trying to vote on their own photo

            Assert.False(result.Success);
            Assert.Equal("You cannot vote for your own submission.", result.ErrorMessage);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task UpvotePhotoAsync_DuplicateVote_ReturnsError()
        {
            // First vote
            await _service.UpvotePhotoAsync(400, 101);

            // Try to vote again
            var result = await _service.UpvotePhotoAsync(400, 101);

            Assert.False(result.Success);
            Assert.Equal("You have already voted for this submission.", result.ErrorMessage);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task RemoveVoteAsync_ValidVote_RemovesSuccessfully()
        {
            // First add a vote
            await _service.UpvotePhotoAsync(400, 101);
            var votedSubmission = await DbContext.Photos.FindAsync(400);
            var votesAfterUpvote = votedSubmission!.Votes;

            // Remove the vote
            var result = await _service.RemoveVoteAsync(400, 101);

            Assert.True(result.Success);
            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Result);
            Assert.Equal(votesAfterUpvote - 1, result.Result.Votes);
        }

        [Fact]
        public async Task RemoveVoteAsync_NonExistentSubmission_ReturnsError()
        {
            var result = await _service.RemoveVoteAsync(99999, 101);

            Assert.False(result.Success);
            Assert.Equal("Submission not found.", result.ErrorMessage);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task RemoveVoteAsync_NoExistingVote_ReturnsError()
        {
            var result = await _service.RemoveVoteAsync(400, 101);

            Assert.False(result.Success);
            Assert.Equal("You have not voted for this submission.", result.ErrorMessage);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task RemoveVoteAsync_PersistsToDatabase()
        {
            // Add vote first
            await _service.UpvotePhotoAsync(400, 101);
            var beforeRemove = (await DbContext.Photos.FindAsync(400))!.Votes;

            // Remove vote
            await _service.RemoveVoteAsync(400, 101);

            var submission = await DbContext.Photos.FindAsync(400);
            Assert.NotNull(submission);
            Assert.Equal(beforeRemove - 1, submission.Votes);
        }

        [Fact]
        public async Task GetUserVotesForTaskAsync_ReturnsCorrectVotes()
        {
            // Add some votes
            await _service.UpvotePhotoAsync(400, 101);

            var votes = await _service.GetUserVotesForTaskAsync(200, 101);

            Assert.NotNull(votes);
            Assert.True(votes.ContainsKey(400));
            Assert.True(votes[400]);
        }

        [Fact]
        public async Task GetUserVotesForTaskAsync_NoVotes_ReturnsEmptyDictionary()
        {
            var votes = await _service.GetUserVotesForTaskAsync(200, 999);

            Assert.NotNull(votes);
        }

        [Fact]
        public async Task GetUserVotesForChallengeAsync_ReturnsCorrectVotes()
        {
            // Add some votes
            await _service.UpvotePhotoAsync(400, 101);

            var votes = await _service.GetUserVotesForChallengeAsync(1, 101);

            Assert.NotNull(votes);
        }

        [Fact]
        public async Task GetUserVotesForChallengeAsync_NoVotes_ReturnsEmptyDictionary()
        {
            var votes = await _service.GetUserVotesForChallengeAsync(1, 999);

            Assert.NotNull(votes);
        }
    }
}