using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Exceptions;

namespace PhotoScavengerHunt.Services
{
    public class ChallengeService
    {
        private readonly PhotoScavengerHuntDbContext _dbContext;

        public ChallengeService(PhotoScavengerHuntDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private static string NormalizeCode(string code) =>
            (code ?? string.Empty).Trim().ToUpperInvariant();
        private string GenerateJoinCode()
        {
            // Generate a random 6-character alphanumeric code
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<string> GenerateUniqueJoinCodeAsync()
        {
            string code;
            do
            {
                code = GenerateJoinCode();
            } while (await _dbContext.Challenges.AnyAsync(h => h.JoinCode == code));

            return code;
        }

        public async Task<Challenge> CreateChallengeAsync(CreateChallengeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ChallengeValidationException("Challenge name cannot be empty.");

            if (!await _dbContext.Users.AnyAsync(u => u.Id == request.CreatorId))
                throw new ChallengeNotFoundException("Creator user does not exist.");

            if (!await _dbContext.Tasks.AnyAsync(t => t.Id == request.TaskId))
                throw new ChallengeNotFoundException("Task does not exist.");

            // Check if user already created a challenge (admin role)
            var adminCount = await _dbContext.ChallengeParticipants
                .Where(cp => cp.UserId == request.CreatorId && cp.Role == ChallengeRole.Admin)
                .CountAsync();

            if (adminCount >= 1)
                throw new ChallengeLimitException("A user can create only one challenge at a time.");

            // Validate deadline (must be in future, max 7 days ahead)
            if (request.Deadline.HasValue)
            {
                var now = DateTime.UtcNow;
                if (request.Deadline.Value <= now)
                    throw new ChallengeValidationException("Deadline must be in the future.");

                var maxDeadline = now.AddDays(7);
                if (request.Deadline.Value > maxDeadline)
                    throw new ChallengeValidationException("Deadline cannot be more than 7 days from now.");
            }
            
            var joinCode = await GenerateUniqueJoinCodeAsync();

            var challenge = ChallengeFactory.Create(
                name: request.Name,
                taskId: request.TaskId,
                creatorId: request.CreatorId,
                isPrivate: request.IsPrivate,
                joinCode: joinCode,
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
            return challenge;
        }

        public async Task<ChallengeParticipant> JoinChallengeAsync(JoinChallengeRequest request)
        {
            var code = NormalizeCode(request.JoinCode);
            if (string.IsNullOrWhiteSpace(code))
                throw new ChallengeValidationException("Join code cannot be empty.");

            var challenge = await _dbContext.Challenges
                .FirstOrDefaultAsync(h => h.JoinCode == code);

            if (challenge == null)
                throw new ChallengeNotFoundException("Challenge not found.");

            if (!await _dbContext.Users.AnyAsync(u => u.Id == request.UserId))
                throw new ChallengeNotFoundException("User does not exist.");

            var count = await _dbContext.ChallengeParticipants
                .Where(cp => cp.UserId == request.UserId)
                .CountAsync();

            if (count >= 6)
                throw new ChallengeLimitException("A user can participate in at most 6 challenges at a time.");

            var existingAny = await _dbContext.ChallengeParticipants
                .FirstOrDefaultAsync(cp => cp.UserId == request.UserId && cp.ChallengeId == challenge.Id);

            if (existingAny != null)
                throw new ChallengeValidationException("User is already a participant in this challenge.");

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

            return participant;
        }

        public async Task<List<Challenge>> GetChallengesAsync(bool publicOnly = true, ChallengeSortBy sortBy = ChallengeSortBy.CreatedAtDesc)
        {
            var query = _dbContext.Challenges.AsQueryable();
            if (publicOnly)
                query = query.Where(c => !c.IsPrivate);

            // Use the generic sorter with multiple constraints
            query = query.SortBy(sortBy);

            var challenges = await query.ToListAsync();
            foreach (var c in challenges)
                c.Participants = null;

            return challenges;
        }

        public async Task<Challenge> GetChallengeByIdAsync(int challengeId)
        {
            var challenge = await _dbContext.Challenges.FirstOrDefaultAsync(c => c.Id == challengeId);
            if (challenge == null)
                throw new ChallengeNotFoundException("Challenge not found.");

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

            return challenge;
        }

        public async Task DeleteChallengeAsync(int challengeId, int userId)
        {
            var challenge = await _dbContext.Challenges.FirstOrDefaultAsync(c => c.Id == challengeId);
            if (challenge == null)
                throw new ChallengeNotFoundException("Challenge not found.");

            var participant = await _dbContext.ChallengeParticipants
                .FirstOrDefaultAsync(cp => cp.ChallengeId == challengeId && cp.UserId == userId);

            if (participant == null || participant.Role != ChallengeRole.Admin)
                throw new ChallengeValidationException("Only challenge admins can delete challenges.");

            var allParticipants = await _dbContext.ChallengeParticipants
                .Where(cp => cp.ChallengeId == challengeId)
                .ToListAsync();

            _dbContext.ChallengeParticipants.RemoveRange(allParticipants);
            _dbContext.Challenges.Remove(challenge);
            await _dbContext.SaveChangesAsync();
        }

        public async Task LeaveChallengeAsync(int challengeId, int userId)
        {
            var participant = await _dbContext.ChallengeParticipants
                .FirstOrDefaultAsync(cp => cp.ChallengeId == challengeId && cp.UserId == userId);

            if (participant == null)
                throw new ChallengeNotFoundException("User is not a participant of this challenge.");

            var challenge = await _dbContext.Challenges.FirstOrDefaultAsync(c => c.Id == challengeId);
            if (challenge == null)
                throw new ChallengeNotFoundException("Challenge not found.");

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
                }
                else
                {
                    var newAdmin = otherParticipants.First();
                    newAdmin.Role = ChallengeRole.Admin;
                    _dbContext.ChallengeParticipants.Remove(participant);
                    await _dbContext.SaveChangesAsync();
                }
            }
            else
            {
                _dbContext.ChallengeParticipants.Remove(participant);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<Challenge> AdvanceChallengeAsync(int challengeId, int requestingUserId)
        {
            var challenge = await _dbContext.Challenges.FirstOrDefaultAsync(c => c.Id == challengeId);
            if (challenge == null)
                throw new ChallengeNotFoundException("Challenge not found.");

            // only creator or admin participant can advance
            if (challenge.CreatorId != requestingUserId)
            {
                var isAdmin = await _dbContext.ChallengeParticipants
                    .AnyAsync(cp => cp.ChallengeId == challengeId && cp.UserId == requestingUserId && cp.Role == ChallengeRole.Admin);
                if (!isAdmin)
                    throw new ChallengeValidationException("Not authorized to advance challenge stage.");
            }

            if (challenge.Status == ChallengeStatus.Open)
            {
                challenge.Status = ChallengeStatus.Closed; // move to voting
                await _dbContext.SaveChangesAsync();
                return challenge;
            }

            if (challenge.Status == ChallengeStatus.Closed)
            {
                var finalized = await FinalizeChallengeAsync(challengeId);
                return finalized;
            }

            throw new ChallengeValidationException("Challenge is already completed.");
        }

        public async Task<Challenge> FinalizeChallengeAsync(int challengeId)
        {
            var challenge = await _dbContext.Challenges.FirstOrDefaultAsync(c => c.Id == challengeId);
            if (challenge == null)
                throw new ChallengeNotFoundException("Challenge not found.");

            if (challenge.WinnerId != null && challenge.Status == ChallengeStatus.Completed)
                return challenge;

            var top = await _dbContext.Photos
                .Where(p => p.ChallengeId == challengeId)
                .GroupBy(p => p.UserId)
                .Select(g => new { UserId = g.Key, TotalVotes = g.Sum(p => p.Votes) })
                .OrderByDescending(x => x.TotalVotes)
                .ThenBy(x => x.UserId)
                .FirstOrDefaultAsync();

            var winnerId = top?.UserId;
            if (winnerId.HasValue)
            {
                challenge.WinnerId = winnerId.Value;
                challenge.Status = ChallengeStatus.Completed;

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == winnerId.Value);
                if (user != null)
                {
                    user.Wins += 1;
                }
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                challenge.WinnerId = null;
                challenge.Status = ChallengeStatus.Completed;
                await _dbContext.SaveChangesAsync();
            }

            return challenge;
        }
        
        public async Task<List<Challenge>> GetChallengesForUserAsync(int userId)
        {
            var challengeIds = await _dbContext.ChallengeParticipants
                .Where(cp => cp.UserId == userId)
                .Select(cp => cp.ChallengeId)
                .Distinct()
                .ToListAsync();

            if (!challengeIds.Any())
                return new List<Challenge>();

            var challenges = await _dbContext.Challenges
                .Where(c => challengeIds.Contains(c.Id))
                .ToListAsync();

            foreach (var c in challenges)
            {
                var participants = await _dbContext.ChallengeParticipants
                    .Where(cp => cp.ChallengeId == c.Id)
                    .ToListAsync();
                c.Participants = participants;
            }

            return challenges;
        }
    }
}