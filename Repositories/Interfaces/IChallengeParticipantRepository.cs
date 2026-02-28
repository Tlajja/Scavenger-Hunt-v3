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
    Task EnsureUserCanJoinChallengeAsync(int userId, int challengeId, int maxParticipants);
    Task<ChallengeParticipant> EnsureParticipantExistsAsync(int challengeId, int userId);
    Task EnsureUserCanAdvanceAsync(int challengeId, int userId);
    Task SaveChangesAsync();
}