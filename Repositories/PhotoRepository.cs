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
    }
}