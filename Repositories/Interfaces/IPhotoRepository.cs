using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Repositories;

public interface IPhotoRepository
{
    Task<List<PhotoSubmission>> GetByChallengeAsync(int challengeId);
    Task AddAsync(PhotoSubmission submission);
    Task SaveChangesAsync();
    Task<(bool Success, string? ErrorMessage, PhotoSubmission? Result)> UpvoteAsync(int submissionId);
    Task<PhotoSubmission?> GetSubmissionWithCommentsAsync(int submissionId);
    Task<PhotoSubmission> EnsureSubmissionExistsAsync(int submissionId);
    Task AddCommentAsync(Comment comment);
    Task RemoveCommentAsync(Comment comment);
    Task<List<PhotoSubmission>> GetSubmissionsForTaskAsync(int taskId);
    Task<List<PhotoSubmission>> GetSubmissionsByUserAsync(int userId);
    Task<List<PhotoSubmission>> GetSubmissionsForChallengeAsync(int challengeId);
    Task<PhotoSubmission?> FindByIdAsync(int submissionId);
    Task RemoveAsync(PhotoSubmission submission);
}