using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Services.Interfaces;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/comments/{commentId}/reactions")]
    public class CommentReactionsController : ControllerBase
    {
        private readonly ICommentReactionService _reactionService;

        public CommentReactionsController(ICommentReactionService reactionService)
        {
            _reactionService = reactionService;
        }

        [HttpPost]
        public async Task<IActionResult> AddReaction(int commentId, [FromBody] AddReactionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Emoji))
            {
                return BadRequest("Emoji cannot be empty.");
            }

            var result = await _reactionService.AddReactionAsync(commentId, request.UserId, request.Emoji);
            if (!result.Success)
            {
                if (result.Error.Contains("not found", StringComparison.InvariantCultureIgnoreCase))
                    return NotFound(result.Error);

                return BadRequest(result.Error);
            }

            return Ok(result.Reactions);
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveReaction(int commentId, [FromQuery] int userId, [FromQuery] string emoji)
        {
            if (string.IsNullOrWhiteSpace(emoji))
            {
                return BadRequest("Emoji cannot be empty.");
            }

            var result = await _reactionService.RemoveReactionAsync(commentId, userId, emoji);
            if (!result.Success)
            {
                if (result.Error.Contains("not found", StringComparison.InvariantCultureIgnoreCase))
                    return NotFound(result.Error);

                return BadRequest(result.Error);
            }

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetReactions(int commentId)
        {
            var result = await _reactionService.GetReactionsAsync(commentId);
            if (!result.Success)
                return NotFound(result.Error);

            return Ok(result.Reactions);
        }
    }

    public class AddReactionRequest
    {
        public int UserId { get; set; }
        public string Emoji { get; set; } = "";
    }
}
