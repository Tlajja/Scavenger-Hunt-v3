using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Leaderboard;

namespace PhotoScavengerHunt.Repositories;

public class LeaderboardRepository : ILeaderboardRepository
{
    private readonly PhotoScavengerHuntDbContext _dbContext;

    public LeaderboardRepository(PhotoScavengerHuntDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync()
    {
        var leaderboard = await _dbContext.Photos
            .GroupBy(p => p.UserId)
            .Select(g => new LeaderboardEntry
            {
                UserId = g.Key,
                UserName = _dbContext.Users
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

    public async Task<List<LeaderboardEntry>> GetHallOfFameAsync(int top = 10)
    {
        var users = await _dbContext.Users
            .OrderByDescending(u => u.Wins)
            .ThenBy(u => u.Id)
            .Take(top)
            .Select(u => new LeaderboardEntry(u.Id, u.Name ?? "Unknown", u.Wins))
            .ToListAsync();
        
        users.Sort();
        return users;
    }
}