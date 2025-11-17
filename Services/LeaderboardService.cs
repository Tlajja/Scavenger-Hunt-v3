using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Leaderboard;
using PhotoScavengerHunt.Services.Interfaces;

namespace PhotoScavengerHunt.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly PhotoScavengerHuntDbContext dbContext;
        private readonly ILogger<LeaderboardService> _logger;

        public LeaderboardService(PhotoScavengerHuntDbContext dbContext, ILogger<LeaderboardService> logger)
        {
            this.dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<LeaderboardEntry>> GetLeaderboardAsync()
        {
            try
            {
                var leaderboard = await dbContext.Photos
                    .GroupBy(p => p.UserId)
                    .Select(g => new LeaderboardEntry
                    {
                        UserId = g.Key,
                        UserName = dbContext.Users
                            .Where(u => u.Id == g.Key)
                            .Select(u => u.Name)
                            .FirstOrDefault() ?? "Unknown",
                        TotalVotes = g.Sum(p => p.Votes)
                    })
                    .ToListAsync();

                // Sort using IComparable<LeaderboardEntry>
                leaderboard.Sort();

                return leaderboard;
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
                var users = await dbContext.Users
                    .OrderByDescending(u => u.Wins)
                    .ThenBy(u => u.Id)
                    .Take(top)
                    .Select(u => new LeaderboardEntry(u.Id, u.Name ?? "Unknown", u.Wins))
                    .ToListAsync();
                
                users.Sort();
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Hall of Fame.");
                throw;
            }
        }
    }
}