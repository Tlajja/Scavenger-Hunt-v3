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
            // Include ChallengeTasks so downstream logic (e.g., photo upload) can resolve a TaskId.
            return await _dbContext.Challenges
                .Include(c => c.ChallengeTasks)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Challenge?> GetWithParticipantsAsync(int id)
        {
            return await _dbContext.Challenges
                .Include(c => c.Participants)
                .Include(c => c.ChallengeTasks)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task<List<Challenge>> GetByIdsAsync(IEnumerable<int> ids)
        {
            return await _dbContext.Challenges
                .Where(c => ids.Contains(c.Id))
                .Include(c => c.ChallengeTasks)
                .ToListAsync();
        }
        public async Task<List<Challenge>> GetAllAsync(bool publicOnly = true, ChallengeSortBy sortBy = ChallengeSortBy.CreatedAtDesc)
        {
            var query = _dbContext.Challenges.AsQueryable();
            if(publicOnly) query = query.Where(c => !c.IsPrivate);

            // Use the generic sorter with multiple constraints
            query = query.SortBy(sortBy);

            return await query
                .Include(c => c.ChallengeTasks)
                .ToListAsync();
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
            var challenge = await _dbContext.Challenges
                .Include(c => c.ChallengeTasks)
                .FirstOrDefaultAsync(c => c.JoinCode == joinCode);
            if (challenge == null)
                throw new EntityNotFoundException("Challenge with the provided join code does not exist.");
            return challenge;
        }

        public async Task DeleteCascadeAsync(int challengeId)
        {
            var challenge = await _dbContext.Challenges
                .Include(c => c.Participants)
                .Include(c => c.ChallengeTasks)
                .FirstOrDefaultAsync(c => c.Id == challengeId);
            if (challenge == null)
                throw new EntityNotFoundException("Challenge not found.");

            var taskIds = challenge.ChallengeTasks.Select(ct => ct.TaskId).Distinct().ToList();
            var exclusiveTaskIds = await _dbContext.ChallengeTasks
                .Where(ct => taskIds.Contains(ct.TaskId))
                .GroupBy(ct => ct.TaskId)
                .Where(g => g.Count() == 1 && g.Any(x => x.ChallengeId == challengeId))
                .Select(g => g.Key)
                .ToListAsync();

            var photos = await _dbContext.Photos.Where(p => p.ChallengeId == challengeId).ToListAsync();
            if (photos.Any())
            {
                _dbContext.Photos.RemoveRange(photos);
            }

            if (challenge.Participants != null && challenge.Participants.Any())
                _dbContext.ChallengeParticipants.RemoveRange(challenge.Participants);

            if (challenge.ChallengeTasks != null && challenge.ChallengeTasks.Any())
                _dbContext.ChallengeTasks.RemoveRange(challenge.ChallengeTasks);

            if (exclusiveTaskIds.Any())
            {
                var tasks = await _dbContext.Tasks.Where(t => exclusiveTaskIds.Contains(t.Id)).ToListAsync();
                if (tasks.Any())
                {
                    _dbContext.Tasks.RemoveRange(tasks);
                }
            }

            _dbContext.Challenges.Remove(challenge);
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
                .Include(c => c.ChallengeTasks)
                .FirstOrDefaultAsync(c => c.Id == challengeId);
            if (challenge == null)
                throw new EntityNotFoundException("Challenge not found.");
            return challenge;
        }

        public async Task<List<int>> GetTopUsersByVotesAsync(int challengeId)
        {
            var grouped = await _dbContext.Photos
                .Where(p => p.ChallengeId == challengeId)
                .GroupBy(p => p.UserId)
                .Select(g => new { UserId = g.Key, Total = g.Sum(p => p.Votes) })
                .Where(x => x.Total > 0)
                .ToListAsync();

            if (grouped == null || grouped.Count == 0) return new List<int>();

            var max = grouped.Max(x => x.Total);
            return grouped.Where(x => x.Total == max).Select(x => x.UserId).ToList();
        }
    }
}