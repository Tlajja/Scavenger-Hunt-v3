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

        public async Task<(bool Success, string? ErrorMessage, PhotoSubmission? Result)> UpvoteAsync(int submissionId)
        {
            try
            {
                var submission = await _dbContext.Photos.FindAsync(submissionId);
                if (submission == null)
                    return (false, "Submission not found.", null);

                submission.Votes += 1;
                await _dbContext.SaveChangesAsync();
                return (true, null, submission);
            }
            catch (DbUpdateException dbEx)
            {
                var msg = dbEx.InnerException?.Message ?? dbEx.Message;
                return (false, $"Database error: {msg}", null);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }

        public async Task<PhotoSubmission?> GetSubmissionWithCommentsAsync(int submissionId)
        {
            return await _dbContext.Photos
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == submissionId);
        }

        public async Task<PhotoSubmission> EnsureSubmissionExistsAsync(int submissionId)
        {
            var submission = await GetSubmissionWithCommentsAsync(submissionId);
            if (submission == null)
                throw new ArgumentException("Submission not found.");
            return submission;
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