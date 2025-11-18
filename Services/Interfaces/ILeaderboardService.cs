using System.Collections.Generic;
using PhotoScavengerHunt.Features.Leaderboard;

namespace PhotoScavengerHunt.Services.Interfaces;

public interface ILeaderboardService
{
    Task<List<LeaderboardEntry>> GetLeaderboardAsync();
    Task<List<LeaderboardEntry>> GetHallOfFameAsync(int top = 10);
}

