using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Services.Interfaces;
using PhotoScavengerHunt.Repositories;
using System.Linq;

namespace PhotoScavengerHunt.Services
{
    public class CommentService : ICommentService
    {
        private readonly IPhotoRepository _photoRepo;
        private readonly ILogger<CommentService> _logger;

        public CommentService(IPhotoRepository photoRepo, ILogger<CommentService> logger)
        {
            _photoRepo = photoRepo;
            _logger = logger;
        }

        public async Task<(bool Success, string Error, List<Comment>? Comments)> AddCommentAsync(int submissionId, AddCommentRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Text))
                    return (false, "Comment text cannot be empty.", null);

                var submission = await _photoRepo.EnsureSubmissionExistsAsync(submissionId);

                var comment = new Comment
                {
                    UserId = request.UserId,
                    Text = request.Text,
                    Timestamp = DateTime.UtcNow,
                    PhotoSubmissionId = submissionId
                };

                await _photoRepo.AddCommentAsync(comment);
                await _photoRepo.SaveChangesAsync();

                submission = await _photoRepo.GetSubmissionWithCommentsAsync(submissionId) ?? submission;

                _logger.LogInformation("Comment added by user {UserId} to submission {SubmissionId}", request.UserId, submissionId);

                return (true, "", submission.Comments.ToList());
            }
            catch (ArgumentException aex)
            {
                return (false, aex.Message, null);
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
                var submission = await _photoRepo.GetSubmissionWithCommentsAsync(submissionId);
                if (submission == null)
                    return (false, "Submission not found.", null);

                // Get usernames for all comment authors
                var userIds = submission.Comments.Select(c => c.UserId).ToList();
                var userNames = await _photoRepo.GetUserNamesAsync(userIds);

                var processedComments = submission.Comments
                    .Select(comment => new
                    {
                        comment.Id,
                        comment.UserId,
                        UserName = userNames.GetValueOrDefault(comment.UserId, $"User {comment.UserId}"),
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
                var submission = await _photoRepo.EnsureSubmissionExistsAsync(submissionId);

                var commentToRemove = submission.Comments.FirstOrDefault(c => c.Id == commentId);
                if (commentToRemove == null)
                    return (false, "Comment not found.");

                await _photoRepo.RemoveCommentAsync(commentToRemove);
                await _photoRepo.SaveChangesAsync();

                _logger.LogInformation("Comment {CommentId} deleted from submission {SubmissionId}", commentId, submissionId);
                return (true, "");
            }
            catch (ArgumentException aex)
            {
                return (false, aex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId} from submission {SubmissionId}", commentId, submissionId);
                return (false, "An unexpected error occurred while deleting the comment.");
            }
        }
    }
}