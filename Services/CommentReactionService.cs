using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Exceptions;
using PhotoScavengerHunt.Services.Interfaces;
using PhotoScavengerHunt.Repositories;

namespace PhotoScavengerHunt.Services
{
    public class CommentReactionService : ICommentReactionService
    {
        private readonly PhotoScavengerHuntDbContext _context;
        private readonly IUserRepository _userRepo;
        private readonly ILogger<CommentReactionService> _logger;
        private readonly IHubContext<CommentsHub>? _commentsHub;

        public CommentReactionService(
            PhotoScavengerHuntDbContext context,
            IUserRepository userRepo,
            ILogger<CommentReactionService> logger,
            IHubContext<CommentsHub>? commentsHub = null)
        {
            _context = context;
            _userRepo = userRepo;
            _logger = logger;
            _commentsHub = commentsHub;
        }

        private async Task<List<CommentReaction>> ProcessReactionsWithUsernamesAsync(List<CommentReaction> reactions)
        {
            if (reactions == null || !reactions.Any())
                return new List<CommentReaction>();

            var userIds = reactions.Select(r => r.UserId).Distinct().ToList();
            var userNames = await _userRepo.GetUserNamesAsync(userIds);

            foreach (var reaction in reactions)
            {
                reaction.UserName = userNames.GetValueOrDefault(reaction.UserId, $"User {reaction.UserId}");
            }

            return reactions;
        }

        public async Task<(bool Success, string Error, List<CommentReaction>? Reactions)> AddReactionAsync(int commentId, int userId, string emoji)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(emoji))
                    return (false, "Emoji cannot be empty.", null);

                if (emoji.Length > 10)
                    return (false, "Emoji is too long.", null);

                var comment = await _context.Comments
                    .Include(c => c.Reactions)
                    .FirstOrDefaultAsync(c => c.Id == commentId);

                if (comment == null)
                    throw new EntityNotFoundException("Comment not found.");

                // Check if user already has a reaction with this emoji
                var existingReaction = comment.Reactions
                    .FirstOrDefault(r => r.UserId == userId && r.Emoji == emoji);

                if (existingReaction != null)
                    return (false, "You have already reacted with this emoji.", null);

                // Remove any other reaction from this user (user can only have one reaction per comment)
                var userOtherReactions = comment.Reactions
                    .Where(r => r.UserId == userId && r.Emoji != emoji)
                    .ToList();

                foreach (var oldReaction in userOtherReactions)
                {
                    _context.CommentReactions.Remove(oldReaction);
                }

                var reaction = new CommentReaction
                {
                    CommentId = commentId,
                    UserId = userId,
                    Emoji = emoji,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CommentReactions.Add(reaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reaction {Emoji} added by user {UserId} to comment {CommentId}", emoji, userId, commentId);

                // Get updated reactions
                var updatedReactions = await _context.CommentReactions
                    .Where(r => r.CommentId == commentId)
                    .ToListAsync();

                var processedReactions = await ProcessReactionsWithUsernamesAsync(updatedReactions);

                // Notify via SignalR
                if (_commentsHub != null)
                {
                    var submission = await _context.Comments
                        .Where(c => c.Id == commentId)
                        .Select(c => c.PhotoSubmissionId)
                        .FirstOrDefaultAsync();

                    await _commentsHub
                        .Clients
                        .Group(CommentsHub.GetSubmissionGroupName(submission))
                        .SendAsync("ReactionsUpdated", commentId);
                }

                return (true, "", processedReactions);
            }
            catch (EntityNotFoundException nfex)
            {
                return (false, nfex.Message, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding reaction to comment {CommentId}", commentId);
                return (false, "An unexpected error occurred while adding the reaction.", null);
            }
        }

        public async Task<(bool Success, string Error)> RemoveReactionAsync(int commentId, int userId, string emoji)
        {
            try
            {
                var reaction = await _context.CommentReactions
                    .FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId && r.Emoji == emoji);

                if (reaction == null)
                    return (false, "Reaction not found.");

                _context.CommentReactions.Remove(reaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reaction {Emoji} removed by user {UserId} from comment {CommentId}", emoji, userId, commentId);

                // Notify via SignalR
                if (_commentsHub != null)
                {
                    var submission = await _context.Comments
                        .Where(c => c.Id == commentId)
                        .Select(c => c.PhotoSubmissionId)
                        .FirstOrDefaultAsync();

                    await _commentsHub
                        .Clients
                        .Group(CommentsHub.GetSubmissionGroupName(submission))
                        .SendAsync("ReactionsUpdated", commentId);
                }

                return (true, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing reaction from comment {CommentId}", commentId);
                return (false, "An unexpected error occurred while removing the reaction.");
            }
        }

        public async Task<(bool Success, string Error, List<CommentReaction>? Reactions)> GetReactionsAsync(int commentId)
        {
            try
            {
                var reactions = await _context.CommentReactions
                    .Where(r => r.CommentId == commentId)
                    .ToListAsync();

                var processedReactions = await ProcessReactionsWithUsernamesAsync(reactions);
                return (true, "", processedReactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reactions for comment {CommentId}", commentId);
                return (false, "An unexpected error occurred while fetching reactions.", null);
            }
        }
    }
}
