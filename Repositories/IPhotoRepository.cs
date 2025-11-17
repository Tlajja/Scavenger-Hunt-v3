using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Repositories;

public interface IPhotoRepository
{
    Task<List<PhotoSubmission>> GetByChallengeAsync(int challengeId);
    Task AddAsync(PhotoSubmission submission);
    Task SaveChangesAsync();
    Task<(bool Success, string? ErrorMessage, PhotoSubmission? Result)> UpvoteAsync(int submissionId);
}