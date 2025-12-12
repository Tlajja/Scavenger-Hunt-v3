using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Services.Interfaces;

public interface IVotesService
{
    Task<(bool Success, string? ErrorMessage, PhotoSubmission? Result)> UpvotePhotoAsync(int submissionId, int userId);
}

