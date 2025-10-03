using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionsController : ControllerBase
    {
        private static readonly List<PhotoSubmission> submissions = new();
        private static int nextSubmissionId = 1;

        // Submit a photo
        [HttpPost]
        public IActionResult SubmitPhoto(int taskId, int userId, string photoUrl)
        {
            var submission = new PhotoSubmission
            {
                Id = nextSubmissionId++,
                TaskId = taskId,
                UserId = userId,
                PhotoUrl = photoUrl,
                Votes = 0,
                Comments = new List<Comment>()
            };

            submissions.Add(submission);

            return CreatedAtAction(nameof(GetSubmissionsForTask), new { taskId }, submission);
        }

        // Get all submissions for a specific task
        [HttpGet("{taskId}")]
        public IEnumerable<PhotoSubmission> GetSubmissionsForTask(int taskId) =>
            submissions.Where(s => s.TaskId == taskId);

        // Get all submissions by a specific user
        [HttpGet("user/{userId}")]
        public IEnumerable<PhotoSubmission> GetSubmissionsByUser(int userId) =>
            submissions.Where(s => s.UserId == userId);

        // Upvote a photo submission
        [HttpPost("{id}/vote")]
        public IActionResult UpvotePhoto(int id)
        {
            var submission = submissions.FirstOrDefault(s => s.Id == id);
            if (submission == null) return NotFound();

            submission.Votes += 1;

            return Ok(submission);
        }

        public class AddCommentRequest
        {
            public int UserId { get; set; }
            public string Text { get; set; } = "";
        }

        // Add a comment to a photo submission
        [HttpPost("{id}/comment")]
        public IActionResult AddComment(int id, [FromBody] AddCommentRequest request)
        {
            var submission = submissions.FirstOrDefault(s => s.Id == id);
            if (submission == null) return NotFound();

            submission.Comments.Add(new Comment
            {
                UserId = request.UserId,
                Text = request.Text,
                Timestamp = DateTime.UtcNow
            });

            return Ok(submission.Comments);
        }
    }
}
