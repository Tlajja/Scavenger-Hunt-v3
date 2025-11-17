using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Exceptions;

namespace PhotoScavengerHunt.Repositories
{
    public class ChallengeParticipantRepository : IChallengeParticipantRepository
    {
        private readonly PhotoScavengerHuntDbContext _dbContext;
        public ChallengeParticipantRepository(PhotoScavengerHuntDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CountAdminChallengesForUserAsync(int userId) 
        {
            return await _dbContext.ChallengeParticipants
                     .Where(cp => cp.UserId == userId && cp.Role == ChallengeRole.Admin)
                     .CountAsync();
        }
        public async Task AddAsync(ChallengeParticipant p)
        {
            await _dbContext.ChallengeParticipants.AddAsync(p);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ChallengeParticipant>> GetByChallengeAsync(int challengeId)
        {
            return await _dbContext.ChallengeParticipants.Where(cp => cp.ChallengeId == challengeId).ToListAsync();
        }

        public async Task<ChallengeParticipant?> GetParticipantAsync(int challengeId, int userId)
        {
            return await _dbContext.ChallengeParticipants
                .FirstOrDefaultAsync(cp => cp.ChallengeId == challengeId && cp.UserId == userId);
        }

        public async Task<List<ChallengeParticipant>> GetByUserAsync(int userId)
        {
            return await _dbContext.ChallengeParticipants.Where(cp => cp.UserId == userId).ToListAsync();
        }

        public async Task RemoveAsync(ChallengeParticipant participant)
        {
            _dbContext.ChallengeParticipants.Remove(participant);
            await _dbContext.SaveChangesAsync();
        }

        public async Task EnsureUserCanCreateChallengeAsync(int userId)
        {
            var adminCount = await CountAdminChallengesForUserAsync(userId);
            if (adminCount >= 1)
                throw new ChallengeLimitException("A user can create only one challenge at a time.");
        }

        public async Task EnsureUserCanJoinChallengeAsync(int userId, int challengeId)
        {
            // check if user exists
            var exists = await _dbContext.Users.AnyAsync(u => u.Id == userId);
            if (!exists) throw new ChallengeNotFoundException("User does not exist.");

            var count = await _dbContext.ChallengeParticipants
                .Where(cp => cp.UserId == userId)
                .CountAsync();
            if (count >= 6)
                throw new ChallengeLimitException("A user can participate in at most 6 challenges at a time.");

            var existingAny = await _dbContext.ChallengeParticipants
                .FirstOrDefaultAsync(cp => cp.UserId == userId && cp.ChallengeId == challengeId);
            if (existingAny != null)
                throw new ChallengeValidationException("User is already a participant in this challenge.");
        }

        public async Task TransferAdminAsync(int challengeId, int fromUserId, int toUserId)
        {
            var from = await GetParticipantAsync(challengeId, fromUserId);
            var to = await GetParticipantAsync(challengeId, toUserId);
            if (from == null || to == null)
                throw new ChallengeNotFoundException("Participant(s) not found for transfer.");

            if (from.Role != ChallengeRole.Admin)
                throw new ChallengeValidationException("Source user is not an admin.");

            from.Role = ChallengeRole.Participant;
            to.Role = ChallengeRole.Admin;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<ChallengeParticipant> EnsureParticipantExistsAsync(int challengeId, int userId)
        {
            var p = await _dbContext.ChallengeParticipants
                .FirstOrDefaultAsync(cp => cp.ChallengeId == challengeId && cp.UserId == userId);
            if (p == null)
                throw new ChallengeNotFoundException("User is not a participant of this challenge.");
            return p;
        }

        public async Task EnsureUserCanAdvanceAsync(int challengeId, int userId)
        {
            // load challenge to check creator
            var challenge = await _dbContext.Challenges.FirstOrDefaultAsync(c => c.Id == challengeId);
            if (challenge == null)
                throw new ChallengeNotFoundException("Challenge not found.");

            if (challenge.CreatorId == userId) return; // creator can always advance

            var participant = await _dbContext.ChallengeParticipants
                .FirstOrDefaultAsync(cp => cp.ChallengeId == challengeId && cp.UserId == userId);
            if (participant == null || participant.Role != ChallengeRole.Admin)
                throw new ChallengeValidationException("Not authorized to advance challenge stage.");
        }
    }
}