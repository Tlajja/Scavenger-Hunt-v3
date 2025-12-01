using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Services.Interfaces;
using PhotoScavengerHunt.Repositories;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IUserRepository _userRepo;

        public CommentsController(ICommentService comService, IUserRepository userRepo)
        {
            _commentService = comService;
            _userRepo = userRepo;
        }

        [HttpPost("{submissionId}")]
        public async Task<IActionResult> AddComment(int submissionId, [FromBody] AddCommentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Comment text cannot be empty.\n");
            }

            var result = await _commentService.AddCommentAsync(submissionId, request);
            if (!result.Success)
                return BadRequest(result.Error);

            var comments = result.Comments ?? new List<Comment>();

            // Get usernames for all comments
            var userIds = comments.Select(c => c.UserId).Distinct().ToList();
            var userNames = await _userRepo.GetUserNamesAsync(userIds);

            var processedComments = new List<object>();
            foreach (var c in comments)
            {
                Console.WriteLine($"User {c.UserId} commented at {c.Timestamp}: {c.Text}");
                var processedComment = new
                {
                    c.Id,
                    c.UserId,
                    UserName = userNames.GetValueOrDefault(c.UserId, $"User {c.UserId}"),
                    c.Text,
                    c.Timestamp,
                    IsRecent = c.Timestamp > DateTime.UtcNow.AddHours(-24),
                    Preview = c.Text?.Length > 50
                        ? c.Text.Substring(0, 50) + "..."
                        : c.Text
                };
                processedComments.Add(processedComment);
            }

            return Ok(processedComments);
        }

        [HttpGet("{submissionId}")]
        public async Task<IActionResult> GetCommentsForSubmission(int submissionId)
        {
            var result = await _commentService.GetCommentsAsync(submissionId);
            if (!result.Success)
                return NotFound(result.Error);

            var rawComments = result.Comments ?? new List<object>();
            var processedComments = new List<object>();

            foreach (var item in rawComments)
            {
                if (item is Comment comment)
                {
                    var processedComment = new
                    {
                        comment.Id,
                        comment.UserId,
                        comment.Text,
                        comment.Timestamp,
                        IsRecent = comment.Timestamp > DateTime.UtcNow.AddHours(-24),
                        Preview = comment.Text?.Length > 50
                            ? comment.Text.Substring(0, 50) + "..."
                            : comment.Text
                    };

                    Console.WriteLine($"User {comment.UserId} commented at {comment.Timestamp}: {comment.Text}");
                    processedComments.Add(processedComment);
                }
                else
                {
                    processedComments.Add(item);
                }
            }

            return Ok(processedComments);
        }

        [HttpDelete("{submissionId}/{commentId}")]
        public async Task<IActionResult> DeleteComment(int submissionId, int commentId)
        {
            var result = await _commentService.DeleteCommentAsync(submissionId, commentId);
            if (!result.Success)
                return NotFound(result.Error);

            return NoContent();
        }
    }
}
