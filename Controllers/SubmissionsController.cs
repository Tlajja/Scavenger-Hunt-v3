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
        public async Task<IActionResult> SubmitPhoto([FromBody] PhotoSubmission req)
        {
            // Validation
            if (req == null || string.IsNullOrWhiteSpace(req.PhotoUrl))
            {
                return BadRequest("Photo URL cannot be empty.\n");
            }
            if(!await _db.Tasks.AnyAsync(t => t.Id == req.TaskId))
            {
                return BadRequest("Task does not exist.\n");
            }
            if(!await _db.Users.AnyAsync(u => u.Id == req.UserId))
            {
                return BadRequest("User does not exist.\n");}

            var submission = new PhotoSubmission
            {
                TaskId = req.TaskId,
                UserId = req.UserId,
                PhotoUrl = req.PhotoUrl,
                Votes = 0,
                Comments = new List<Comment>()
            };

            _db.Photos.Add(submission);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubmissionsForTask), new { taskId = submission.TaskId }, submission);
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
            // Validation
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Comment text cannot be empty.\n");
            }

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

            var processedComments = new List<Comment>();
            foreach (var c in submission.Comments)
            {
                Console.WriteLine($"User {c.UserId} commented at {c.Timestamp}: {c.Text}");
                
                processedComments.Add(c);
            }

            return Ok(processedComments);
        }

        // Get comments for a specific submission with foreach processing
        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetCommentsForSubmission(int id)
        {
            var submission = await _db.Photos
                .Include(s => s.Comments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null) return NotFound();

            var processedComments = new List<object>();
            foreach (var comment in submission.Comments)
            {
                var processedComment = new
                {
                    Id = comment.Id,
                    UserId = comment.UserId,
                    Text = comment.Text,
                    Timestamp = comment.Timestamp,
                    IsRecent = comment.Timestamp > DateTime.UtcNow.AddHours(-24),
                    Preview = comment.Text.Length > 50 ? comment.Text.Substring(0, 50) + "..." : comment.Text
                };
                
                processedComments.Add(processedComment);
            }

            return Ok(processedComments);
        }

        // Get comments by specific user with foreach filtering
        [HttpGet("{id}/comments/user/{userId}")]
        public async Task<IActionResult> GetCommentsByUser(int id, int userId)
        {
            var submission = await _db.Photos
                .Include(s => s.Comments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null) return NotFound();

            // Filtering comments by userId using foreach
            var userComments = new List<Comment>();
            foreach (var comment in submission.Comments)
            {
                if (comment.UserId == userId)
                {
                    userComments.Add(comment);
                }
            }

            return Ok(userComments);
        }
    }
}
