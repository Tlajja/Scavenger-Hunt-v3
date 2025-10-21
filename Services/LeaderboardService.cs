using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Leaderboard;

namespace PhotoScavengerHunt.Services
{
    public class LeaderboardService
    {
        private readonly PhotoScavengerHuntDbContext _db;
        private readonly ILogger<LeaderboardService> _logger;

        public LeaderboardService(PhotoScavengerHuntDbContext db, ILogger<LeaderboardService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<LeaderboardEntry>> GetLeaderboardAsync()
        {
            try
            {
                var leaderboard = await _db.Photos
                    .GroupBy(p => p.UserId)
                    .Select(g => new LeaderboardEntry
                    {
                        UserId = g.Key,
                        UserName = _db.Users
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
    }
}