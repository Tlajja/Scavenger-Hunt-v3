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

            var result = await _service.UpvotePhotoAsync(400);

            Assert.True(result.Success);
            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Result);
            Assert.Equal(initialVotes + 1, result.Result.Votes);
        }

        [Fact]
        public async Task UpvotePhotoAsync_MultipleVotes_IncrementsCorrectly()
        {
            var initialVotes = (await DbContext.Photos.FindAsync(400))!.Votes;

            await _service.UpvotePhotoAsync(400);
            await _service.UpvotePhotoAsync(400);
            var result = await _service.UpvotePhotoAsync(400);

            Assert.Equal(initialVotes + 3, result.Result!.Votes);
        }

        [Fact]
        public async Task UpvotePhotoAsync_NonExistentSubmission_ReturnsError()
        {
            var result = await _service.UpvotePhotoAsync(99999);

            Assert.False(result.Success);
            Assert.Equal("Submission not found.", result.ErrorMessage);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task UpvotePhotoAsync_PersistsToDatabase()
        {
            await _service.UpvotePhotoAsync(400);

            var submission = await DbContext.Photos.FindAsync(400);

            Assert.NotNull(submission);
            Assert.Equal(6, submission.Votes); 
        }

        [Fact]
        public async Task UpvotePhotoAsync_ReturnsUpdatedSubmission()
        {
            var result = await _service.UpvotePhotoAsync(400);

            Assert.NotNull(result.Result);
            Assert.Equal(400, result.Result.Id);
            Assert.Equal(200, result.Result.TaskId);
            Assert.Equal(100, result.Result.UserId);
        }
    }
}
