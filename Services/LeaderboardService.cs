using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Leaderboard;
using PhotoScavengerHunt.Services.Interfaces;
using PhotoScavengerHunt.Repositories;

namespace PhotoScavengerHunt.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ILeaderboardRepository _leaderboardRepo;
        private readonly ILogger<LeaderboardService> _logger;

        public LeaderboardService(ILeaderboardRepository leaderboardRepo, ILogger<LeaderboardService> logger)
        {
            _leaderboardRepo = leaderboardRepo;
            _logger = logger;
        }

        public async Task<List<LeaderboardEntry>> GetLeaderboardAsync()
        {
            try
            {
                return await _leaderboardRepo.GetLeaderboardAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leaderboard data.");
                throw new ApplicationException("Unable to retrieve leaderboard data at this time.", ex);
            }
        }

        public async Task<List<LeaderboardEntry>> GetHallOfFameAsync(int top = 10)
        {
            try
            {
                return await _leaderboardRepo.GetHallOfFameAsync(top);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Hall of Fame.");
                throw;
            }
        }
    }
}