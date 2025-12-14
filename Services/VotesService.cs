using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Services.Interfaces;
using PhotoScavengerHunt.Repositories;

namespace PhotoScavengerHunt.Services
{
    public class VotesService : IVotesService
    {
        private readonly IPhotoRepository _photoRepo;

        public VotesService(IPhotoRepository photoRepo)
        {
            _photoRepo = photoRepo;
        }

        public async Task<(bool Success, string? ErrorMessage, PhotoSubmission? Result)> UpvotePhotoAsync(int submissionId, int userId)
        {
            var submission = await _photoRepo.FindByIdAsync(submissionId);
            if (submission == null)
                return (false, "Submission not found.", null);
            if (submission.UserId == userId)
                return (false, "You cannot vote for your own submission.", null);

            var result = await _photoRepo.UpvoteAsync(submissionId);
            if (!result.Success) 
                return result;
            await _photoRepo.SaveChangesAsync();
            return (true, null, result.Result);
        }
    }
}