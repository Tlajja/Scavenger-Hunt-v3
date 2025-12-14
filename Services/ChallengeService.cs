using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Exceptions;
using PhotoScavengerHunt.Repositories;
using PhotoScavengerHunt.Services.Interfaces;

namespace PhotoScavengerHunt.Services
{
    public class ChallengeService : IChallengeService
    {
        private readonly IChallengeRepository _challengeRepo;
        private readonly IUserRepository _userRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly IChallengeParticipantRepository _participantRepo;
        private readonly IPhotoRepository _photoRepo;
        private readonly IStorageService _storageService;

        public ChallengeService(
            IChallengeRepository challengeRepo,
            IUserRepository userRepo,
            ITaskRepository taskRepo,
            IChallengeParticipantRepository participantRepo,
            IPhotoRepository photoRepo,
            IStorageService storageService)
        {
            _challengeRepo = challengeRepo;
            _userRepo = userRepo;
            _taskRepo = taskRepo;
            _participantRepo = participantRepo;
            _photoRepo = photoRepo;
            _storageService = storageService;
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
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Challenge name cannot be empty.");

            if(!await _userRepo.ExistsAsync(request.CreatorId))
                throw new EntityNotFoundException("User does not exist.");

            var taskIdList = request.TaskIds?.Distinct().ToList() ?? new List<int>();
            if (!taskIdList.Any())
                throw new ValidationException("At least one task must be provided for the challenge.");
            foreach (var tid in taskIdList)
            {
                if(!await _taskRepo.ExistsAsync(tid))
                    throw new EntityNotFoundException("Task does not exist.");
            }

            // ensure deadline is valid
            if (request.Deadline.HasValue)
            {
                var now = DateTime.UtcNow;
                if (request.Deadline.Value <= now)
                    throw new ValidationException("Deadline must be in the future.");

                var maxDeadline = now.AddDays(7);
                if (request.Deadline.Value > maxDeadline)
                    throw new ValidationException("Deadline cannot be more than 7 days from now.");
            }

            // check if user can create challenge
            var adminCount = await _participantRepo.CountAdminChallengesForUserAsync(request.CreatorId);
            if (adminCount >= 1)
                throw new LimitExceededException("A user can create only one challenge at a time.");
            
            var joinCode = await GenerateUniqueJoinCodeAsync();

            var challenge = ChallengeFactory.Create(
                name: request.Name,
                creatorId: request.CreatorId,
                taskIds: taskIdList,
                isPrivate: request.IsPrivate,
                joinCode: joinCode,
                deadline: request.Deadline,
                maxParticipants: request.MaxParticipants);

            await _challengeRepo.AddAsync(challenge);
            await _challengeRepo.SaveChangesAsync();

            if (challenge.ChallengeTasks?.Count > 0)
            {
                var taskIds = challenge.ChallengeTasks.Select(ct => ct.TaskId).ToList();
                var tasks = await _taskRepo.GetByIdsAsync(taskIds);
                var taskMap = tasks.ToDictionary(t => t.Id, t => t);
                foreach (var ct in challenge.ChallengeTasks)
                {
                    if (taskMap.TryGetValue(ct.TaskId, out var t) && t.TimerSeconds.HasValue && t.TimerSeconds.Value > 0)
                    {
                        ct.Deadline = challenge.CreatedAt.AddSeconds(t.TimerSeconds.Value);
                    }
                    else
                    {
                        ct.Deadline = null;
                    }
                }
                await _challengeRepo.SaveChangesAsync();
            }

            var participant = new ChallengeParticipant
            {
                ChallengeId = challenge.Id,
                UserId = request.CreatorId,
                Role = ChallengeRole.Admin,
                JoinedAt = DateTime.UtcNow
            };

            await _participantRepo.AddAsync(participant);
            await _participantRepo.SaveChangesAsync();
            challenge.Participants = new List<ChallengeParticipant> { participant };
            return challenge;
        }

        public async Task<ChallengeParticipant> JoinChallengeAsync(JoinChallengeRequest request)
        {
            var code = NormalizeCode(request.JoinCode);
            if (string.IsNullOrWhiteSpace(code))
                throw new ValidationException("Join code cannot be empty.");

            var challenge = await _challengeRepo.GetByJoinCodeAsync(code);

            await _participantRepo.EnsureUserCanJoinChallengeAsync(request.UserId, challenge.Id, challenge.MaxParticipants ?? 10);

            var participant = new ChallengeParticipant
            {
                ChallengeId = challenge.Id,
                UserId = request.UserId,
                Role = ChallengeRole.Participant,
                JoinedAt = DateTime.UtcNow
            };

            await _participantRepo.AddAsync(participant);
            await _participantRepo.SaveChangesAsync();
            participant.Challenge = null;
            participant.User = null;
            return participant;
        }

        public async Task<List<Challenge>> GetChallengesAsync(bool publicOnly = true, ChallengeSortBy sortBy = ChallengeSortBy.CreatedAtDesc)
        {
            var challenges = await _challengeRepo.GetAllAsync(publicOnly, sortBy);

            foreach (var c in challenges)
            {
                c.Participants = null;
                // Load participant count for display
                var participants = await _participantRepo.GetByChallengeAsync(c.Id);
                c.Participants = participants; // Temporarily set for count, will be cleared if needed
            }

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

            var photos = await _photoRepo.GetByChallengeAsync(challengeId);
            if (photos != null && photos.Any())
            {
                foreach (var p in photos)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(p.PhotoUrl))
                        {
                            await _storageService.DeleteFileAsync(p.PhotoUrl);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            await _challengeRepo.DeleteCascadeAsync(challengeId);
            await _challengeRepo.SaveChangesAsync();
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
                    await _challengeRepo.DeleteCascadeAsync(challengeId);
                    await _challengeRepo.SaveChangesAsync();
                    return;
                }

                var newAdmin = otherParticipants.First();

                // transfer admin role
                var from = await _participantRepo.GetParticipantAsync(challengeId, userId);
                var to = await _participantRepo.GetParticipantAsync(challengeId, newAdmin.UserId);
                if (from == null || to == null)
                    throw new EntityNotFoundException("Participant(s) not found for transfer.");

                if (from.Role != ChallengeRole.Admin)
                    throw new ValidationException("Source user is not an admin.");

                from.Role = ChallengeRole.Participant;
                to.Role = ChallengeRole.Admin;
                await _participantRepo.SaveChangesAsync();

                await _participantRepo.RemoveAsync(participant);
                await _participantRepo.SaveChangesAsync();
                return;
            }

            await _participantRepo.RemoveAsync(participant);
            await _participantRepo.SaveChangesAsync();
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

            throw new ValidationException("Challenge is already completed.");
        }

        public async Task<Challenge> FinalizeChallengeAsync(int challengeId)
        {
            var challenge = await _challengeRepo.EnsureChallengeExistsAsync(challengeId);

            if (challenge.WinnerId != null && challenge.Status == ChallengeStatus.Completed)
                return challenge;

            var topUsers = await _challengeRepo.GetTopUsersByVotesAsync(challengeId);
            challenge.Status = ChallengeStatus.Completed;
            if (topUsers != null && topUsers.Count > 0)
            {
                challenge.WinnerId = topUsers.Count == 1 ? topUsers[0] : (int?)null;

                foreach (var uid in topUsers)
                {
                    var user = await _userRepo.GetByIdAsync(uid);
                    if (user == null)
                        throw new EntityNotFoundException("User does not exist.");
                    user.Wins += 1;
                }
            }
            else
            {
                challenge.WinnerId = null;
            }

            await _challengeRepo.SaveChangesAsync();

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