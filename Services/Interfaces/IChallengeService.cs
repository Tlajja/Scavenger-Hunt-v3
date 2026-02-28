using System.Collections.Generic;
using PhotoScavengerHunt.Features.Challenges;

namespace PhotoScavengerHunt.Services.Interfaces;

public interface IChallengeService
{
    Task<Challenge> CreateChallengeAsync(CreateChallengeRequest request);
    Task<ChallengeParticipant> JoinChallengeAsync(JoinChallengeRequest request);
    Task<List<Challenge>> GetChallengesAsync(bool publicOnly = true, ChallengeSortBy sortBy = ChallengeSortBy.CreatedAtDesc);
    Task<Challenge> GetChallengeByIdAsync(int challengeId);
    Task DeleteChallengeAsync(int challengeId, int userId);
    Task LeaveChallengeAsync(int challengeId, int userId);
    Task<Challenge> AdvanceChallengeAsync(int challengeId, int requestingUserId);
    Task<Challenge> FinalizeChallengeAsync(int challengeId);
    Task<List<Challenge>> GetChallengesForUserAsync(int userId);
}

