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
                return BadRequest("Comment text cannot be empty.");

            var submission = await _db.Photos.Include(s => s.Comments)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                return NotFound("Submission not found.");

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

        [HttpGet("{submissionId}")]
        public async Task<IActionResult> GetComments(int submissionId)
        {
            var submission = await _db.Photos
                .Include(s => s.Comments)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                return NotFound();

            var result = submission.Comments.Select(c => new
            {
                c.Id,
                c.UserId,
                c.Text,
                c.Timestamp,
                IsRecent = c.Timestamp > DateTime.UtcNow.AddHours(-24),
                Preview = c.Text.Length > 50 ? c.Text[..50] + "..." : c.Text
            });

            return Ok(result);
        }

        [HttpDelete("{submissionId}/{commentId}")]
        public async Task<IActionResult> DeleteComment(int submissionId, int commentId)
        {
            var submission = await _db.Photos.Include(s => s.Comments)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                return NotFound("Submission not found.");

            var comment = submission.Comments.FirstOrDefault(c => c.Id == commentId);
            if (comment == null)
                return NotFound("Comment not found.");

            submission.Comments.Remove(comment);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
