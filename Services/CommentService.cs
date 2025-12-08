using Microsoft.AspNetCore.SignalR;
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
        private readonly IUserRepository _userRepo;
        private readonly ILogger<CommentService> _logger;
        private readonly IHubContext<CommentsHub>? _commentsHub;

        public CommentService(
            IPhotoRepository photoRepo,
            IUserRepository userRepo,
            ILogger<CommentService> logger,
            IHubContext<CommentsHub>? commentsHub = null)
        {
            _photoRepo = photoRepo;
            _userRepo = userRepo;
            _logger = logger;
            _commentsHub = commentsHub;
        }

        private async Task<List<object>> ProcessCommentsWithUsernamesAsync(List<Comment> comments)
        {
            if (comments == null || !comments.Any())
                return new List<object>();

            var userIds = comments.Select(c => c.UserId).ToList();
            var userNames = await _userRepo.GetUserNamesAsync(userIds);

            return comments
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
        }

        public async Task<(bool Success, string Error, List<object>? Comments)> AddCommentAsync(int submissionId, AddCommentRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Text))
                    return (false, "Comment text cannot be empty.", null);

                var submission = await _photoRepo.GetSubmissionWithCommentsAsync(submissionId);
                if (submission == null)
                    throw new ArgumentException("Submission not found.");

                var comment = new Comment
                {
                    UserId = request.UserId,
                    Text = request.Text,
                    Timestamp = DateTime.UtcNow,
                    PhotoSubmissionId = submissionId
                };

                await _photoRepo.AddCommentAsync(comment);
                await _photoRepo.SaveChangesAsync();

                submission = await _photoRepo.GetSubmissionWithCommentsAsync(submissionId);
                if (submission == null)
                    throw new ArgumentException("Submission not found after adding comment.");

                _logger.LogInformation("Comment added by user {UserId} to submission {SubmissionId}", request.UserId, submissionId);

                var processedComments = await ProcessCommentsWithUsernamesAsync(submission.Comments.ToList());

                if (_commentsHub != null)
                {
                    await _commentsHub
                        .Clients
                        .Group(CommentsHub.GetSubmissionGroupName(submissionId))
                        .SendAsync("CommentsUpdated", submissionId);
                }

                return (true, "", processedComments);
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

                var processedComments = await ProcessCommentsWithUsernamesAsync(submission.Comments.ToList());
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
                var submission = await _photoRepo.GetSubmissionWithCommentsAsync(submissionId);
                if (submission == null)
                    throw new ArgumentException("Submission not found.");

                var commentToRemove = submission.Comments.FirstOrDefault(c => c.Id == commentId);
                if (commentToRemove == null)
                    return (false, "Comment not found.");

                await _photoRepo.RemoveCommentAsync(commentToRemove);
                await _photoRepo.SaveChangesAsync();

                if (_commentsHub != null)
                {
                    await _commentsHub
                        .Clients
                        .Group(CommentsHub.GetSubmissionGroupName(submissionId))
                        .SendAsync("CommentsUpdated", submissionId);
                }

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