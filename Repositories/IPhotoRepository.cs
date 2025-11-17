using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Repositories;

public interface IPhotoRepository
{
    Task<List<PhotoSubmission>> GetByChallengeAsync(int challengeId);
    Task AddAsync(PhotoSubmission submission);
    Task SaveChangesAsync();
}