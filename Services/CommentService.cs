using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Services
{
    public class CommentService
    {
        private readonly PhotoScavengerHuntDbContext _db;
        private readonly ILogger<CommentService> _logger;

        public CommentService(PhotoScavengerHuntDbContext db, ILogger<CommentService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<(bool Success, string Error, List<Comment>? Comments)> AddCommentAsync(int submissionId, AddCommentRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Text))
                    return (false, "Comment text cannot be empty.", null);

                var submission = await _db.Photos
                    .Include(s => s.Comments)
                    .FirstOrDefaultAsync(s => s.Id == submissionId);

                if (submission == null)
                    return (false, "Submission not found.", null);

                var comment = new Comment
                {
                    UserId = request.UserId,
                    Text = request.Text,
                    Timestamp = DateTime.UtcNow,
                    PhotoSubmissionId = submissionId
                };

                submission.Comments.Add(comment);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Comment added by user {UserId} to submission {SubmissionId}", request.UserId, submissionId);

                return (true, "", submission.Comments.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to submission {SubmissionId}", submissionId);
                return (false, "An unexpected error occurred while adding the comment.", null);
            }
        }

        public async Task<(bool Success, string Error, List<object>? Comments)> GetCommentsAsync(int submissionId)
        {
            try
            {
                var submission = await _db.Photos
                    .Include(s => s.Comments)
                    .FirstOrDefaultAsync(s => s.Id == submissionId);

                if (submission == null)
                    return (false, "Submission not found.", null);

                var processedComments = submission.Comments
                    .Select(comment => new
                    {
                        comment.Id,
                        comment.UserId,
                        comment.Text,
                        comment.Timestamp,
                        IsRecent = comment.Timestamp > DateTime.UtcNow.AddHours(-24),
                        Preview = comment.Text.Length > 50
                            ? comment.Text[..50] + "..."
                            : comment.Text
                    })
                    .Cast<object>()
                    .ToList();

                return (true, "", processedComments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments for submission {SubmissionId}", submissionId);
                return (false, "An unexpected error occurred while fetching comments.", null);
            }
        }

        public async Task<(bool Success, string Error)> DeleteCommentAsync(int submissionId, int commentId)
        {
            try
            {
                var submission = await _db.Photos
                    .Include(s => s.Comments)
                    .FirstOrDefaultAsync(s => s.Id == submissionId);

                if (submission == null)
                    return (false, "Submission not found.");

                var commentToRemove = submission.Comments.FirstOrDefault(c => c.Id == commentId);
                if (commentToRemove == null)
                    return (false, "Comment not found.");

                submission.Comments.Remove(commentToRemove);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Comment {CommentId} deleted from submission {SubmissionId}", commentId, submissionId);
                return (true, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId} from submission {SubmissionId}", commentId, submissionId);
                return (false, "An unexpected error occurred while deleting the comment.");
            }
        }
    }
}