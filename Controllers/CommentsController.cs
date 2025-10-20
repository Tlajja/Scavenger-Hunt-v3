using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly PhotoScavengerHuntDbContext _db;

        public CommentsController(PhotoScavengerHuntDbContext db)
        {
            _db = db;
        }

        [HttpPost("{submissionId}")]
        public async Task<IActionResult> AddComment(int submissionId, [FromBody] AddCommentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Comment text cannot be empty.\n");
            }

            var submission = await _db.Photos
                .Include(s => s.Comments)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                return NotFound("Submission not found.");

            var comment = new Comment
            {
                UserId = request.UserId,
                Text = request.Text,
                Timestamp = DateTime.UtcNow,
                PhotoSubmissionId = submissionId
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

        [HttpGet("{submissionId}")]
        public async Task<IActionResult> GetCommentsForSubmission(int submissionId)
        {
            var submission = await _db.Photos
                .Include(s => s.Comments)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                return NotFound("Submission not found.");

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
                    Preview = comment.Text.Length > 50
                        ? comment.Text.Substring(0, 50) + "..."
                        : comment.Text
                };

                processedComments.Add(processedComment);
            }

            return Ok(processedComments);
        }


        [HttpDelete("{submissionId}/{commentId}")]
        public async Task<IActionResult> DeleteComment(int submissionId, int commentId)
        {
            var submission = await _db.Photos
                .Include(s => s.Comments)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                return NotFound("Submission not found.");

            var commentToRemove = submission.Comments.FirstOrDefault(c => c.Id == commentId);
            if (commentToRemove == null)
                return NotFound("Comment not found.");

            submission.Comments.Remove(commentToRemove);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}