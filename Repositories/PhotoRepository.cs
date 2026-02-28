using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Repositories
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly PhotoScavengerHuntDbContext _dbContext;
        public PhotoRepository(PhotoScavengerHuntDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<PhotoSubmission>> GetByChallengeAsync(int challengeId)
        {
            return await _dbContext.Photos
                .Where(p => p.ChallengeId == challengeId)
                .ToListAsync();
        }

        public async Task AddAsync(PhotoSubmission submission)
        {
            await _dbContext.Photos.AddAsync(submission);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task<(bool Success, string? ErrorMessage, PhotoSubmission? Result)> UpvoteAsync(int submissionId, int userId)
        {
            try
            {
                var submission = await _dbContext.Photos.FindAsync(submissionId);
                if (submission == null)
                    return (false, "Submission not found.", null);

                // Create vote record
                var vote = new Vote
                {
                    PhotoSubmissionId = submissionId,
                    UserId = userId,
                    VotedAt = DateTime.UtcNow
                };

                await _dbContext.Votes.AddAsync(vote);
                submission.Votes += 1;
                
                return (true, null, submission);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, PhotoSubmission? Result)> RemoveVoteAsync(int submissionId, int userId)
        {
            try
            {
                var vote = await _dbContext.Votes
                    .FirstOrDefaultAsync(v => v.PhotoSubmissionId == submissionId && v.UserId == userId);
                
                if (vote == null)
                    return (false, "Vote not found.", null);

                var submission = await _dbContext.Photos.FindAsync(submissionId);
                if (submission == null)
                    return (false, "Submission not found.", null);

                _dbContext.Votes.Remove(vote);
                submission.Votes = Math.Max(0, submission.Votes - 1);

                return (true, null, submission);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }

        public async Task<Vote?> GetUserVoteForSubmissionAsync(int submissionId, int userId)
        {
            return await _dbContext.Votes
                .FirstOrDefaultAsync(v => v.PhotoSubmissionId == submissionId && v.UserId == userId);
        }

        public async Task<Dictionary<int, bool>> GetUserVotesForTaskAsync(int taskId, int userId)
        {
            var submissionIds = await _dbContext.Photos
                .Where(p => p.TaskId == taskId)
                .Select(p => p.Id)
                .ToListAsync();

            var votes = await _dbContext.Votes
                .Where(v => submissionIds.Contains(v.PhotoSubmissionId) && v.UserId == userId)
                .Select(v => v.PhotoSubmissionId)
                .ToListAsync();

            return submissionIds.ToDictionary(id => id, id => votes.Contains(id));
        }

        public async Task<Dictionary<int, bool>> GetUserVotesForChallengeAsync(int challengeId, int userId)
        {
            var submissionIds = await _dbContext.Photos
                .Where(p => p.ChallengeId == challengeId)
                .Select(p => p.Id)
                .ToListAsync();

            var votes = await _dbContext.Votes
                .Where(v => submissionIds.Contains(v.PhotoSubmissionId) && v.UserId == userId)
                .Select(v => v.PhotoSubmissionId)
                .ToListAsync();

            return submissionIds.ToDictionary(id => id, id => votes.Contains(id));
        }

        public async Task<PhotoSubmission?> GetSubmissionWithCommentsAsync(int submissionId)
        {
            return await _dbContext.Photos
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == submissionId);
        }

        public async Task AddCommentAsync(Comment comment)
        {
            await _dbContext.Comments.AddAsync(comment);
        }

        public Task RemoveCommentAsync(Comment comment)
        {
            _dbContext.Comments.Remove(comment);
            return Task.CompletedTask;
        }

        public async Task<List<PhotoSubmission>> GetSubmissionsForTaskAsync(int taskId)
        {
            return await _dbContext.Photos
                .Include(p => p.Comments)
                .Where(s => s.TaskId == taskId)
                .ToListAsync();
        }

        public async Task<List<PhotoSubmission>> GetSubmissionsByUserAsync(int userId)
        {
            return await _dbContext.Photos
                .Include(p => p.Comments)
                .Where(s => s.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<PhotoSubmission>> GetSubmissionsForChallengeAsync(int challengeId)
        {
            return await _dbContext.Photos
                .Include(p => p.Comments)
                .Where(p => p.ChallengeId == challengeId)
                .ToListAsync();
        }

        public async Task<PhotoSubmission?> FindByIdAsync(int submissionId)
        {
            return await _dbContext.Photos.FindAsync(submissionId);
        }

        public Task RemoveAsync(PhotoSubmission submission)
        {
            _dbContext.Photos.Remove(submission);
            return Task.CompletedTask;
        }
    }
}