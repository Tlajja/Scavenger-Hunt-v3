using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotoSubmissionsController : ControllerBase
    {
        private readonly PhotoScavengerHuntDbContext _db;

        public PhotoSubmissionsController(PhotoScavengerHuntDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitPhoto([FromBody] PhotoSubmission submission)
        {
            if (submission == null || string.IsNullOrWhiteSpace(submission.PhotoUrl))
                return BadRequest("Photo URL cannot be empty.");

            if (!await _db.Tasks.AnyAsync(t => t.Id == submission.TaskId))
                return BadRequest("Task does not exist.");

            if (!await _db.Users.AnyAsync(u => u.Id == submission.UserId))
                return BadRequest("User does not exist.");

            var newSubmission = new PhotoSubmission
            {
                TaskId = submission.TaskId,
                UserId = submission.UserId,
                PhotoUrl = submission.PhotoUrl,
                Votes = 0,
                Comments = new List<Comment>()
            };

            _db.Photos.Add(newSubmission);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubmissionsForTask), new { taskId = newSubmission.TaskId }, newSubmission);
        }

        [HttpGet("task/{taskId}")]
        public async Task<IEnumerable<PhotoSubmission>> GetSubmissionsForTask(int taskId) =>
            await _db.Photos
                .Include(p => p.Comments)
                .Where(s => s.TaskId == taskId)
                .ToListAsync();

        [HttpGet("user/{userId}")]
        public async Task<IEnumerable<PhotoSubmission>> GetSubmissionsByUser(int userId) =>
            await _db.Photos
                .Include(p => p.Comments)
                .Where(s => s.UserId == userId)
                .ToListAsync();
    }
}
