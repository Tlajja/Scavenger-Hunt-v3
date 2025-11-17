using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Services.Interfaces;

namespace PhotoScavengerHunt.Services
{
    public class VotesService : IVotesService
    {
        private readonly PhotoScavengerHuntDbContext dbContext;

        public VotesService(PhotoScavengerHuntDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<(bool Success, string? ErrorMessage, PhotoSubmission? Result)> UpvotePhotoAsync(int submissionId)
        {
            try
            {
                var submission = await dbContext.Photos.FindAsync(submissionId);

                if (submission == null)
                    return (false, "Submission not found.", null);

                submission.Votes += 1;
                await dbContext.SaveChangesAsync();

                return (true, null, submission);
            }
            catch (DbUpdateException dbEx)
            {
                return (false, $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}", null);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }
    }
}