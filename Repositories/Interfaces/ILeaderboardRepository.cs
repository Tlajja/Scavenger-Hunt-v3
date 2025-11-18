using PhotoScavengerHunt.Features.Leaderboard;

namespace PhotoScavengerHunt.Repositories;

public interface ILeaderboardRepository
{
    Task<List<LeaderboardEntry>> GetLeaderboardAsync();
    Task<List<LeaderboardEntry>> GetHallOfFameAsync(int top = 10);
}