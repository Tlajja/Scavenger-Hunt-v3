using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Exceptions;

namespace PhotoScavengerHunt.Repositories
{
    public class ChallengeRepository : IChallengeRepository
    {
        private readonly PhotoScavengerHuntDbContext _dbContext;

        public ChallengeRepository(PhotoScavengerHuntDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Challenge?> GetByIdAsync(int id)
        {
            return await _dbContext.Challenges.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Challenge?> GetWithParticipantsAsync(int id)
        {
            return await _dbContext.Challenges
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task<List<Challenge>> GetByIdsAsync(IEnumerable<int> ids)
        {
            return await _dbContext.Challenges
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();
        }
        public async Task<List<Challenge>> GetAllAsync(bool publicOnly = true, ChallengeSortBy sortBy = ChallengeSortBy.CreatedAtDesc)
        {
            var query = _dbContext.Challenges.AsQueryable();
            if(publicOnly) query = query.Where(c => !c.IsPrivate);

            // Use the generic sorter with multiple constraints
            query = query.SortBy(sortBy);

            return await query.ToListAsync();
        }

        public async Task AddAsync(Challenge challenge)
        {
            await _dbContext.Challenges.AddAsync(challenge);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> AnyByJoinCodeAsync(string joinCode)
        {
            return await _dbContext.Challenges.AnyAsync(c => c.JoinCode == joinCode);
        }

        public async Task<Challenge> GetByJoinCodeAsync(string joinCode)
        {
            var challenge =  await _dbContext.Challenges.FirstOrDefaultAsync(c => c.JoinCode == joinCode);
            if (challenge == null)
                throw new ChallengeNotFoundException("Challenge with the provided join code does not exist.");
            return challenge;
        }

        public async Task DeleteCascadeAsync(int challengeId)
        {
            var challenge = await _dbContext.Challenges
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == challengeId);
            if (challenge == null)
                throw new ChallengeNotFoundException("Challenge not found.");

            if (challenge.Participants != null && challenge.Participants.Any())
                _dbContext.ChallengeParticipants.RemoveRange(challenge.Participants);

            _dbContext.Challenges.Remove(challenge);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<(int WinnerId, int TotalVotes)?> GetTopUserByVotesAsync(int challengeId)
        {
            var top = await _dbContext.Photos
                .Where(p => p.ChallengeId == challengeId)
                .GroupBy(p => p.UserId)
                .Select(g => new { UserId = g.Key, Total = g.Sum(p => p.Votes) })
                .OrderByDescending(x => x.Total)
                .ThenBy(x => x.UserId)
                .FirstOrDefaultAsync();

            if (top == null) return null;
            return (top.UserId, top.Total);
        }

        public async Task<Challenge> EnsureChallengeExistsAsync(int challengeId)
        {
            var challenge = await _dbContext.Challenges
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == challengeId);
            if (challenge == null)
                throw new ChallengeNotFoundException("Challenge not found.");
            return challenge;
        }
    }
}