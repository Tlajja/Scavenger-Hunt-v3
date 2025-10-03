using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionsController : ControllerBase
    {
        private readonly PhotoScavengerHuntDbContext _db;

        public SubmissionsController(PhotoScavengerHuntDbContext db)
        {
            _db = db;
        }

        // Submit a photo
        [HttpPost]
        public async Task<IActionResult> SubmitPhoto(int taskId, int userId, string photoUrl)
        {
            var submission = new PhotoSubmission
            {
                TaskId = taskId,
                UserId = userId,
                PhotoUrl = photoUrl,
                Votes = 0,
                Comments = new List<Comment>()
            };

            _db.Photos.Add(submission);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubmissionsForTask), new { taskId }, submission);
        }

        // Get all submissions for a specific task
        [HttpGet("{taskId}")]
        public async Task<IEnumerable<PhotoSubmission>> GetSubmissionsForTask(int taskId) =>
            await _db.Photos
                .Include(p => p.Comments)
                .Where(s => s.TaskId == taskId)
                .ToListAsync();

        // Get all submissions by a specific user
        [HttpGet("user/{userId}")]
        public async Task<IEnumerable<PhotoSubmission>> GetSubmissionsByUser(int userId) =>
            await _db.Photos
                .Include(p => p.Comments)
                .Where(s => s.UserId == userId)
                .ToListAsync();

        // Upvote a photo submission
        [HttpPost("{id}/vote")]
        public async Task<IActionResult> UpvotePhoto(int id)
        {
            var submission = await _db.Photos.FindAsync(id);
            if (submission == null) return NotFound();

            submission.Votes += 1;
            await _db.SaveChangesAsync();

            return Ok(submission);
        }

        public class AddCommentRequest
        {
            public int UserId { get; set; }
            public string Text { get; set; } = "";
        }

        // Add a comment to a photo submission
        [HttpPost("{id}/comment")]
        public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentRequest request)
        {
            var submission = await _db.Photos
                .Include(s => s.Comments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null) return NotFound();

            var comment = new Comment
            {
                UserId = request.UserId,
                Text = request.Text,
                Timestamp = DateTime.UtcNow
            };

            submission.Comments.Add(comment);
            await _db.SaveChangesAsync();

            return Ok(submission.Comments);
        }
    }
}
