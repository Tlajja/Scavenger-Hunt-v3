using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Services.Interfaces;

public interface IVotesService
{
    Task<(bool Success, string? ErrorMessage, PhotoSubmission? Result)> UpvotePhotoAsync(int submissionId, int userId);
    Task<(bool Success, string? ErrorMessage, PhotoSubmission? Result)> RemoveVoteAsync(int submissionId, int userId);
    Task<Dictionary<int, bool>> GetUserVotesForTaskAsync(int taskId, int userId);
    Task<Dictionary<int, bool>> GetUserVotesForChallengeAsync(int challengeId, int userId);
}
