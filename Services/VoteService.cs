using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Services
{
    public class VotesService
    {
        private readonly PhotoScavengerHuntDbContext _db;

        public VotesService(PhotoScavengerHuntDbContext db)
        {
            _db = db;
        }

        public async Task<(bool Success, string? ErrorMessage, PhotoSubmission? Result)> UpvotePhotoAsync(int submissionId)
        {
            try
            {
                var submission = await _db.Photos.FindAsync(submissionId);

                if (submission == null)
                    return (false, "Submission not found.", null);

                submission.Votes += 1;
                await _db.SaveChangesAsync();

                return (true, null, submission);
            }
            catch (DbUpdateException dbEx)
            {
                // Handle DB-level errors (constraint violations, etc.)
                return (false, $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}", null);
            }
            catch (Exception ex)
            {
                // Catch-all for unexpected errors
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }
    }
}