using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Exceptions;
using PhotoScavengerHunt.Repositories;

namespace PhotoScavengerHunt.Services
{
    public class ChallengeService
    {
        //private readonly PhotoScavengerHuntDbContext _dbContext;
        private readonly IChallengeRepository _challengeRepo;
        private readonly IUserRepository _userRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly IChallengeParticipantRepository _participantRepo;
        private readonly IPhotoRepository _photoRepo;

        public ChallengeService(
            //PhotoScavengerHuntDbContext dbContext,
            IChallengeRepository challengeRepo,
            IUserRepository userRepo,
            ITaskRepository taskRepo,
            IChallengeParticipantRepository participantRepo,
            IPhotoRepository photoRepo)
        {
            //_dbContext = dbContext;
            _challengeRepo = challengeRepo;
            _userRepo = userRepo;
            _taskRepo = taskRepo;
            _participantRepo = participantRepo;
            _photoRepo = photoRepo;
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
            } while (await _challengeRepo.AnyByJoinCodeAsync(code));

            return code;
        }

        public async Task<Challenge> CreateChallengeAsync(CreateChallengeRequest request)
        {
            await _challengeRepo.EnsureNameNotEmptyAsync(request.Name);
            await _userRepo.EnsureUserExistsAsync(request.CreatorId, "Creator user does not exist.");
            await _taskRepo.EnsureTaskExistsAsync(request.TaskId);
            await _participantRepo.EnsureUserCanCreateChallengeAsync(request.CreatorId);
            await _challengeRepo.EnsureDeadlineIsValidAsync(request.Deadline);
            
            var joinCode = await GenerateUniqueJoinCodeAsync();

            var challenge = ChallengeFactory.Create(
                name: request.Name,
                taskId: request.TaskId,
                creatorId: request.CreatorId,
                isPrivate: request.IsPrivate,
                joinCode: joinCode,
                deadline: request.Deadline);

            await _challengeRepo.AddAsync(challenge);
            await _challengeRepo.SaveChangesAsync();

            var participant = new ChallengeParticipant
            {
                ChallengeId = challenge.Id,
                UserId = request.CreatorId,
                Role = ChallengeRole.Admin,
                JoinedAt = DateTime.UtcNow
            };

            await _participantRepo.AddAsync(participant);

            challenge.Participants = new List<ChallengeParticipant>();
            return challenge;
        }

        public async Task<ChallengeParticipant> JoinChallengeAsync(JoinChallengeRequest request)
        {
            var code = NormalizeCode(request.JoinCode);
            if (string.IsNullOrWhiteSpace(code))
                throw new ChallengeValidationException("Join code cannot be empty.");

            var challenge = await _challengeRepo.GetByJoinCodeAsync(code);

            await _participantRepo.EnsureUserCanJoinChallengeAsync(request.UserId, challenge.Id);

            var participant = new ChallengeParticipant
            {
                ChallengeId = challenge.Id,
                UserId = request.UserId,
                Role = ChallengeRole.Participant,
                JoinedAt = DateTime.UtcNow
            };

            await _participantRepo.AddAsync(participant);
            participant.Challenge = null;
            participant.User = null;
            return participant;
        }

        public async Task<List<Challenge>> GetChallengesAsync(bool publicOnly = true)
        {
            var challenges = await _challengeRepo.GetAllAsync(publicOnly);
            foreach (var c in challenges)
                c.Participants = null;

            return challenges;
        }

        public async Task<Challenge> GetChallengeByIdAsync(int challengeId)
        {
            var challenge = await _challengeRepo.EnsureChallengeExistsAsync(challengeId);
            challenge.Participants = challenge.Participants ?? new List<ChallengeParticipant>();
            return challenge;
        }

        public async Task DeleteChallengeAsync(int challengeId, int userId)
        {
            await _participantRepo.EnsureUserCanAdvanceAsync(challengeId, userId);
            await _challengeRepo.DeleteCascadeAsync(challengeId);
        }

        public async Task LeaveChallengeAsync(int challengeId, int userId)
        {
            var participant = await _participantRepo.EnsureParticipantExistsAsync(challengeId, userId);
            var challenge = await _challengeRepo.EnsureChallengeExistsAsync(challengeId);

            var otherParticipants = await _participantRepo.GetByChallengeAsync(challengeId);
            otherParticipants = otherParticipants.Where(p => p.UserId != userId).OrderBy(p => p.JoinedAt).ToList();

            if (participant.Role == ChallengeRole.Admin)
            {
                if (!otherParticipants.Any())
                {
                    // no other participants -> delete challenge via repository cascade
                    await _challengeRepo.DeleteCascadeAsync(challengeId);
                    return;
                }

                // transfer admin to the earliest joined participant
                var newAdmin = otherParticipants.First();
                await _participantRepo.TransferAdminAsync(challengeId, userId, newAdmin.UserId);
                await _participantRepo.RemoveAsync(participant);
                return;
            }

            // simple leave
            await _participantRepo.RemoveAsync(participant);
        }

        public async Task<Challenge> AdvanceChallengeAsync(int challengeId, int requestingUserId)
        {
            var challenge = await _challengeRepo.EnsureChallengeExistsAsync(challengeId);
            await _participantRepo.EnsureUserCanAdvanceAsync(challengeId, requestingUserId);

            if (challenge.Status == ChallengeStatus.Open)
            {
                challenge.Status = ChallengeStatus.Closed; // move to voting
                await _challengeRepo.SaveChangesAsync();
                return challenge;
            }

            if (challenge.Status == ChallengeStatus.Closed)
            {
                var finalized = await FinalizeChallengeAsync(challengeId);
                return finalized;
            }

            // already completed
            throw new ChallengeValidationException("Challenge is already completed.");
        }

        public async Task<Challenge> FinalizeChallengeAsync(int challengeId)
        {
            var challenge = await _challengeRepo.EnsureChallengeExistsAsync(challengeId);

            if (challenge.WinnerId != null && challenge.Status == ChallengeStatus.Completed)
                return challenge;

            var top = await _challengeRepo.GetTopUserByVotesAsync(challengeId);
            var winnerId = top?.WinnerId;
            if (winnerId.HasValue)
            {
                challenge.WinnerId = winnerId.Value;
                challenge.Status = ChallengeStatus.Completed;

                await _userRepo.IncrementWinsAsync(winnerId.Value);
                await _challengeRepo.SaveChangesAsync();
            }
            else
            {
                challenge.WinnerId = null;
                challenge.Status = ChallengeStatus.Completed;
                await _challengeRepo.SaveChangesAsync();
            }

            return challenge;
        }
        
        public async Task<List<Challenge>> GetChallengesForUserAsync(int userId)
        {
            var parts = await _participantRepo.GetByUserAsync(userId);
            var challengeIds = parts.Select(p => p.ChallengeId).Distinct().ToList();
            if (!challengeIds.Any()) return new List<Challenge>();

            var challenges = await _challengeRepo.GetByIdsAsync(challengeIds);
            foreach (var c in challenges)
            {
                var participants = await _participantRepo.GetByChallengeAsync(c.Id);
                c.Participants = participants;
            }
            return challenges;
        }
    }
}
