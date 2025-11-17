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
    }
}