using Microsoft.AspNetCore.Http;
using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Services.Interfaces;

public interface IPhotoSubmissionService
{
    Task<(bool Success, string Message, string? PhotoUrl, int? SubmissionId)> UploadPhotoAsync(int? taskId, int userId, IFormFile file, int? challengeId = null);
    Task<List<PhotoSubmission>> GetSubmissionsForTaskAsync(int taskId);
    Task<List<PhotoSubmission>> GetSubmissionsByUserAsync(int userId);
    Task<List<PhotoSubmission>> GetSubmissionsForChallengeAsync(int challengeId);
    Task<(bool Success, string Message)> DeleteSubmissionAsync(int submissionId);
}

