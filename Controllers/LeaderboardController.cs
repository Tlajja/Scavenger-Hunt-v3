using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Features.Users;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly PhotoScavengerHuntDbContext _db;

        public LeaderboardController(PhotoScavengerHuntDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<List<LeaderboardEntry>>> GetLeaderboard()
        {
            var leaderboard = await _db.Photos
                .GroupBy(s => s.UserId)
                .Select(g => new LeaderboardEntry
                {
                    UserId = g.Key,
                    UserName = _db.Users.Where(u => u.Id == g.Key).Select(u => u.Name).FirstOrDefault() ?? "Unknown",
                    TotalVotes = g.Sum(s => s.Votes)
                })
                .OrderByDescending(entry => entry.TotalVotes)
                .ToListAsync();
            return Ok(leaderboard);
        }

        public struct LeaderboardEntry
        {
            public int UserId { get; set; }
            public string UserName { get; set; }
            public int TotalVotes { get; set; }

            public LeaderboardEntry(int userId, string userName, int totalVotes)
            {
                UserId = userId;
                UserName = userName;
                TotalVotes = totalVotes;
            }
        }

    }
}