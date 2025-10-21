using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Services;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly CommentService _service;

        public CommentsController(CommentService service)
        {
            _service = service;
        }

        [HttpPost("{submissionId}")]
        public async Task<IActionResult> AddComment(int submissionId, [FromBody] AddCommentRequest request)
        {
            var result = await _service.AddCommentAsync(submissionId, request);
            if (!result.Success)
                return BadRequest(result.Error);

            return Ok(result.Comments);
        }

        [HttpGet("{submissionId}")]
        public async Task<IActionResult> GetCommentsForSubmission(int submissionId)
        {
            var result = await _service.GetCommentsAsync(submissionId);
            if (!result.Success)
                return NotFound(result.Error);

            return Ok(result.Comments);
        }

        [HttpDelete("{submissionId}/{commentId}")]
        public async Task<IActionResult> DeleteComment(int submissionId, int commentId)
        {
            var result = await _service.DeleteCommentAsync(submissionId, commentId);
            if (!result.Success)
                return NotFound(result.Error);

            return NoContent();
        }
    }
}
