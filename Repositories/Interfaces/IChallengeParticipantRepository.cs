using PhotoScavengerHunt.Features.Challenges;

namespace PhotoScavengerHunt.Repositories;

public interface IChallengeParticipantRepository
{
    Task<int> CountAdminChallengesForUserAsync(int userId);
    Task AddAsync(ChallengeParticipant participant);
    Task<ChallengeParticipant?> GetParticipantAsync(int challengeId, int userId);
    Task<List<ChallengeParticipant>> GetByChallengeAsync(int challengeId);
    Task<List<ChallengeParticipant>> GetByUserAsync(int userId);
    Task RemoveAsync(ChallengeParticipant participant);
    Task EnsureUserCanCreateChallengeAsync(int userId);
    Task EnsureUserCanJoinChallengeAsync(int userId, int challengeId);
    Task TransferAdminAsync(int challengeId, int fromUserId, int toUserId);
    Task<ChallengeParticipant> EnsureParticipantExistsAsync(int challengeId, int userId);
    Task EnsureUserCanAdvanceAsync(int challengeId, int userId);
}