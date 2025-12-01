using PhotoScavengerHunt.Features.Challenges;

namespace PhotoScavengerHunt.Repositories;

public interface IChallengeRepository
{
    Task<Challenge?> GetByIdAsync(int id);
    Task<Challenge?> GetWithParticipantsAsync(int id);
    Task<List<Challenge>> GetByIdsAsync(IEnumerable<int> ids);
    Task<List<Challenge>> GetAllAsync(bool publicOnly = true, ChallengeSortBy sortBy = ChallengeSortBy.CreatedAtDesc);
    Task AddAsync(Challenge challenge);
    Task SaveChangesAsync();
    Task DeleteCascadeAsync(int challengeId);
    Task<(int WinnerId, int TotalVotes)?> GetTopUserByVotesAsync(int challengeId);
    Task<bool> AnyByJoinCodeAsync(string joinCode);
    // Get by join code - repository throws ChallengeNotFoundException if missing
    Task<Challenge> GetByJoinCodeAsync(string joinCode);
    Task<Challenge> EnsureChallengeExistsAsync(int challengeId);
}