using Microsoft.AspNetCore.Mvc;
using System;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Services.Interfaces;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService comService)
        {
            _commentService = comService;
        }

        [HttpPost("{submissionId}")]
        public async Task<IActionResult> AddComment(int submissionId, [FromBody] AddCommentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Comment text cannot be empty.");
            }

            var result = await _commentService.AddCommentAsync(submissionId, request);
            if (!result.Success)
            {
                if (string.Equals(result.Error, "Submission not found.", StringComparison.InvariantCultureIgnoreCase))
                    return NotFound(result.Error);

                return BadRequest(result.Error);
            }

            var comments = result.Comments ?? new List<Comment>();
            return Ok(comments);
        }

        [HttpGet("{submissionId}")]
        public async Task<IActionResult> GetCommentsForSubmission(int submissionId)
        {
            var result = await _commentService.GetCommentsAsync(submissionId);
            if (!result.Success)
                return NotFound(result.Error);

            var comments = result.Comments ?? new List<Comment>();
            return Ok(comments);
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