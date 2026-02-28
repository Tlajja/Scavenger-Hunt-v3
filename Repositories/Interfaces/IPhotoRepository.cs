using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Repositories;

public interface IPhotoRepository
{
    Task<List<PhotoSubmission>> GetByChallengeAsync(int challengeId);
    Task AddAsync(PhotoSubmission submission);
    Task SaveChangesAsync();
    Task<(bool Success, string? ErrorMessage, PhotoSubmission? Result)> UpvoteAsync(int submissionId, int userId);
    
    Task<(bool Success, string? ErrorMessage, PhotoSubmission? Result)> RemoveVoteAsync(int submissionId, int userId);
    
    Task<Vote?> GetUserVoteForSubmissionAsync(int submissionId, int userId);
    
    Task<Dictionary<int, bool>> GetUserVotesForTaskAsync(int taskId, int userId);
    
    Task<Dictionary<int, bool>> GetUserVotesForChallengeAsync(int challengeId, int userId);
    
    Task<PhotoSubmission?> GetSubmissionWithCommentsAsync(int submissionId);
    Task AddCommentAsync(Comment comment);
    Task RemoveCommentAsync(Comment comment);
    Task<List<PhotoSubmission>> GetSubmissionsForTaskAsync(int taskId);
    Task<List<PhotoSubmission>> GetSubmissionsByUserAsync(int userId);
    Task<List<PhotoSubmission>> GetSubmissionsForChallengeAsync(int challengeId);
    Task<PhotoSubmission?> FindByIdAsync(int submissionId);
    Task RemoveAsync(PhotoSubmission submission);
}