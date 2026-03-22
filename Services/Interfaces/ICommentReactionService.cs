using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Services.Interfaces;

public interface ICommentReactionService
{
    Task<(bool Success, string Error, List<CommentReaction>? Reactions)> AddReactionAsync(int commentId, int userId, string emoji);
    Task<(bool Success, string Error)> RemoveReactionAsync(int commentId, int userId, string emoji);
    Task<(bool Success, string Error, List<CommentReaction>? Reactions)> GetReactionsAsync(int commentId);
}
