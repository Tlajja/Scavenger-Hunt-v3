using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Services.Interfaces;
using PhotoScavengerHunt.Repositories;

namespace PhotoScavengerHunt.Services
{
    public class PhotoSubmissionService : IPhotoSubmissionService
    {
        private readonly IPhotoRepository _photoRepo;
        private readonly IUserRepository _userRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly IChallengeRepository _challengeRepo;
        private readonly IWebHostEnvironment _env;

        public PhotoSubmissionService(
            IPhotoRepository photoRepo,
            IUserRepository userRepo,
            ITaskRepository taskRepo,
            IChallengeRepository challengeRepo,
            IWebHostEnvironment env)
        {
            _photoRepo = photoRepo;
            _userRepo = userRepo;
            _taskRepo = taskRepo;
            _challengeRepo = challengeRepo;
            _env = env;
        }

        public async Task<(bool Success, string Message, string? PhotoUrl, int? SubmissionId)> UploadPhotoAsync(int? taskId, int userId, IFormFile file, int? challengeId = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return (false, "No file uploaded.", null, null);

                // if challengeId provided, resolve its TaskId
                if (challengeId.HasValue)
                {
                    await _challengeRepo.EnsureChallengeExistsAsync(challengeId.Value);
                    var challenge = await _challengeRepo.GetByIdAsync(challengeId.Value);
                    taskId = challenge?.TaskId;
                }
                if (!taskId.HasValue)
                    return (false, "Task does not exist.", null, null);

                await _taskRepo.EnsureTaskExistsAsync(taskId.Value);
                await _userRepo.EnsureUserExistsAsync(userId);

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                    return (false, "Only image files (.jpg, .jpeg, .png, .gif) are allowed.", null, null);

                if (file.Length > 10_000_000)
                    return (false, "File size cannot exceed 10MB.", null, null);

                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var photoUrl = $"/uploads/{uniqueFileName}";
                var submission = new PhotoSubmission
                {
                    TaskId = taskId.Value,
                    UserId = userId,
                    ChallengeId = challengeId,
                    PhotoUrl = photoUrl,
                    Votes = 0,
                    Comments = new List<Comment>()
                };

                await _photoRepo.AddAsync(submission);
                await _photoRepo.SaveChangesAsync();

                return (true, "Photo uploaded successfully.", photoUrl, submission.Id);
            }
            catch (ArgumentException aex)
            {
                return (false, aex.Message, null, null);
            }
            catch (DbUpdateException dbEx)
            {
                return (false, $"Database error: {dbEx.Message}", null, null);
            }
            catch (IOException ioEx)
            {
                return (false, $"File system error: {ioEx.Message}", null, null);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}", null, null);
            }
        }

        public async Task<List<PhotoSubmission>> GetSubmissionsForTaskAsync(int taskId)
        {
            try
            {
                return await _photoRepo.GetSubmissionsForTaskAsync(taskId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching submissions for task {taskId}: {ex.Message}");
            }
        }

        public async Task<List<PhotoSubmission>> GetSubmissionsByUserAsync(int userId)
        {
            try
            {
                return await _photoRepo.GetSubmissionsByUserAsync(userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching submissions for user {userId}: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteSubmissionAsync(int submissionId)
        {
            try
            {
                var submission = await _photoRepo.FindByIdAsync(submissionId);
                if (submission == null)
                    return (false, "Submission not found.");

                // Delete file from server (if it exists)
                var filePath = Path.Combine(_env.WebRootPath, submission.PhotoUrl.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                await _photoRepo.RemoveAsync(submission);
                await _photoRepo.SaveChangesAsync();

                return (true, "Submission deleted successfully.");
            }
            catch (ArgumentException aex)
            {
                return (false, aex.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting submission: {ex.Message}");
            }
        }

        public async Task<List<PhotoSubmission>> GetSubmissionsForChallengeAsync(int challengeId)
        {
            try
            {
                return await _photoRepo.GetSubmissionsForChallengeAsync(challengeId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching submissions for challenge {challengeId}: {ex.Message}");
            }
        }
    }
}