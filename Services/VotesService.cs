using Microsoft.EntityFrameworkCore;
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

        public async Task<(bool Success, string? ErrorMessage, PhotoSubmission? Result)> UpvotePhotoAsync(int submissionId)
        {
            var result = await _photoRepo.UpvoteAsync(submissionId);
            if (!result.Success) 
                return result;
            await _photoRepo.SaveChangesAsync();
            return (true, null, result.Result);
        }
    }
}