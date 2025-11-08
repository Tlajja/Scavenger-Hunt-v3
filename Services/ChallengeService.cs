using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Challenges;

namespace PhotoScavengerHunt.Services
{
    public class ChallengeService
    {
        private readonly PhotoScavengerHuntDbContext _dbContext;

        public ChallengeService(PhotoScavengerHuntDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool Success, string Error, Challenge? Challenge)> CreateChallengeAsync(CreateChallengeRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                    return (false, "Challenge name cannot be empty.", null);

                if (!await _dbContext.Users.AnyAsync(u => u.Id == request.CreatorId))
                    return (false, "Creator user does not exist.", null);

                var existingAny = await _dbContext.ChallengeParticipants
                    .FirstOrDefaultAsync(cp => cp.UserId == request.CreatorId);
                if (existingAny != null)
                    return (false, "You must leave your current challenge before creating a new one.", null);

                var challenge = ChallengeFactory.Create(
                   name: request.Name,
                   taskId: request.TaskId,
                   creatorId: request.CreatorId,
                   isPrivate: request.IsPrivate,
                   deadline: request.Deadline);

                _dbContext.Challenges.Add(challenge);
                await _dbContext.SaveChangesAsync();

                var participant = new ChallengeParticipant
                {
                    ChallengeId = challenge.Id,
                    UserId = request.CreatorId,
                    Role = ChallengeRole.Admin,
                    JoinedAt = DateTime.UtcNow
                };

                _dbContext.ChallengeParticipants.Add(participant);
                await _dbContext.SaveChangesAsync();

                challenge.Participants = new List<ChallengeParticipant>();
                return (true, string.Empty, challenge);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Error, ChallengeParticipant? Participant)> JoinChallengeAsync(JoinChallengeRequest request)
        {
            try
            {
                var challenge = await _dbContext.Challenges.FirstOrDefaultAsync(c => c.Id == request.ChallengeId);
                if (challenge == null)
                    return (false, "Challenge not found.", null);

                if (!await _dbContext.Users.AnyAsync(u => u.Id == request.UserId))
                    return (false, "User does not exist.", null);

                var existingAny = await _dbContext.ChallengeParticipants
                    .FirstOrDefaultAsync(cp => cp.UserId == request.UserId);

                if (existingAny != null)
                    return (false, "User is already a participant in a challenge. Leave it first.", null);

                var participant = new ChallengeParticipant
                {
                    ChallengeId = challenge.Id,
                    UserId = request.UserId,
                    Role = ChallengeRole.Participant,
                    JoinedAt = DateTime.UtcNow
                };

                _dbContext.ChallengeParticipants.Add(participant);
                await _dbContext.SaveChangesAsync();

                participant.Challenge = null;
                participant.User = null;

                return (true, string.Empty, participant);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Error, List<Challenge>? Challenges)> GetChallengesAsync(bool publicOnly = true)
        {
            try
            {
                var query = _dbContext.Challenges.AsQueryable();
                if (publicOnly)
                    query = query.Where(c => !c.IsPrivate);

                var challenges = await query.ToListAsync();
                foreach (var c in challenges)
                    c.Participants = null;

                return (true, string.Empty, challenges);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Error, Challenge? Challenge)> GetChallengeByIdAsync(int challengeId)
        {
            try
            {
                var challenge = await _dbContext.Challenges.FirstOrDefaultAsync(c => c.Id == challengeId);
                if (challenge == null)
                    return (false, "Challenge not found.", null);

                var participants = await _dbContext.ChallengeParticipants
                    .Where(cp => cp.ChallengeId == challengeId)
                    .ToListAsync();

                challenge.Participants = participants.Select(p => new ChallengeParticipant
                {
                    Id = p.Id,
                    ChallengeId = p.ChallengeId,
                    UserId = p.UserId,
                    Role = p.Role,
                    JoinedAt = p.JoinedAt
                }).ToList();

                return (true, string.Empty, challenge);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Error)> DeleteChallengeAsync(int challengeId, int userId)
        {
            try
            {
                var challenge = await _dbContext.Challenges.FirstOrDefaultAsync(c => c.Id == challengeId);
                if (challenge == null)
                    return (false, "Challenge not found.");

                var participant = await _dbContext.ChallengeParticipants
                    .FirstOrDefaultAsync(cp => cp.ChallengeId == challengeId && cp.UserId == userId);

                if (participant == null || participant.Role != ChallengeRole.Admin)
                    return (false, "Only challenge admins can delete challenges.");

                var allParticipants = await _dbContext.ChallengeParticipants
                    .Where(cp => cp.ChallengeId == challengeId)
                    .ToListAsync();

                _dbContext.ChallengeParticipants.RemoveRange(allParticipants);
                _dbContext.Challenges.Remove(challenge);
                await _dbContext.SaveChangesAsync();

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Error)> LeaveChallengeAsync(int challengeId, int userId)
        {
            try
            {
                var participant = await _dbContext.ChallengeParticipants
                    .FirstOrDefaultAsync(cp => cp.ChallengeId == challengeId && cp.UserId == userId);

                if (participant == null)
                    return (false, "User is not a participant of this challenge.");

                var challenge = await _dbContext.Challenges.FirstOrDefaultAsync(c => c.Id == challengeId);
                if (challenge == null)
                    return (false, "Challenge not found.");

                var otherParticipants = await _dbContext.ChallengeParticipants
                    .Where(cp => cp.ChallengeId == challengeId && cp.UserId != userId)
                    .OrderBy(cp => cp.JoinedAt)
                    .ToListAsync();

                if (participant.Role == ChallengeRole.Admin)
                {
                    if (!otherParticipants.Any())
                    {
                        _dbContext.ChallengeParticipants.Remove(participant);
                        _dbContext.Challenges.Remove(challenge);
                        await _dbContext.SaveChangesAsync();
                        return (true, string.Empty);
                    }
                    else
                    {
                        var newAdmin = otherParticipants.First();
                        newAdmin.Role = ChallengeRole.Admin;
                        _dbContext.ChallengeParticipants.Remove(participant);
                        await _dbContext.SaveChangesAsync();
                        return (true, string.Empty);
                    }
                }
                else
                {
                    _dbContext.ChallengeParticipants.Remove(participant);
                    await _dbContext.SaveChangesAsync();
                    return (true, string.Empty);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}");
            }
        }
    }
}
