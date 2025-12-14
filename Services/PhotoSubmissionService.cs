using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Services.Interfaces;
using PhotoScavengerHunt.Repositories;
using PhotoScavengerHunt.Exceptions;

namespace PhotoScavengerHunt.Services
{
    public class PhotoSubmissionService : IPhotoSubmissionService
    {
        private readonly IPhotoRepository _photoRepo;
        private readonly IUserRepository _userRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly IChallengeRepository _challengeRepo;
        private readonly IStorageService _storage;

        public PhotoSubmissionService(
            IPhotoRepository photoRepo,
            IUserRepository userRepo,
            ITaskRepository taskRepo,
            IChallengeRepository challengeRepo,
            IStorageService storage)
        {
            _photoRepo = photoRepo;
            _userRepo = userRepo;
            _taskRepo = taskRepo;
            _challengeRepo = challengeRepo;
            _storage = storage;
        }

        public async Task<(bool Success, string Message, string? PhotoUrl, int? SubmissionId)> UploadPhotoAsync(int? taskId, int userId, IFormFile file, int? challengeId = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return (false, "No file uploaded.", null, null);

                if (challengeId.HasValue)
                {
                    await _challengeRepo.EnsureChallengeExistsAsync(challengeId.Value);
                    var challenge = await _challengeRepo.GetByIdAsync(challengeId.Value);
                    // If the challenge itself has a deadline and it's expired, block submission
                    if (challenge?.Deadline != null && challenge.Deadline <= DateTime.UtcNow)
                    {
                        return (false, "Challenge deadline has expired. Submissions are closed.", null, null);
                    }
                    // If challenge status is not Open (submission phase), block submission
                    if (challenge != null && challenge.Status != Features.Challenges.ChallengeStatus.Open)
                    {
                        return (false, "Challenge is not in submission phase.", null, null);
                    }
                    if (!taskId.HasValue)
                        taskId = challenge?.ChallengeTasks?.FirstOrDefault()?.TaskId;
                }
                if (!taskId.HasValue)
                    return (false, "Task does not exist.", null, null);

                if(!await _taskRepo.ExistsAsync(taskId.Value))
                    throw new EntityNotFoundException("Task does not exist.");

                // Enforce task/challenge task deadline constraints
                DateTime? effectiveDeadline = null;
                if (challengeId.HasValue)
                {
                    var challenge = await _challengeRepo.GetByIdAsync(challengeId.Value);
                    var meta = challenge?.ChallengeTasks?.FirstOrDefault(ct => ct.TaskId == taskId.Value);
                    effectiveDeadline = meta?.Deadline;
                }

                // Fallback to the task's own deadline if per-challenge deadline is not set
                if (effectiveDeadline == null)
                {
                    var task = await _taskRepo.GetByIdAsync(taskId.Value);
                    effectiveDeadline = task?.Deadline;
                }

                if (effectiveDeadline != null && effectiveDeadline <= DateTime.UtcNow)
                {
                    return (false, "Task deadline has expired. You cannot submit a photo for this task.", null, null);
                }

                if (challengeId.HasValue)
                {
                    var existingForTask = await _photoRepo.GetSubmissionsForTaskAsync(taskId.Value);
                    var duplicate = existingForTask.FirstOrDefault(s =>
                        s.UserId == userId && s.ChallengeId.HasValue && s.ChallengeId.Value == challengeId.Value
                    );
                    if (duplicate != null)
                    {
                        return (false, "You have already submitted a photo for this task in this challenge.", null, null);
                    }
                }
                
                if(!await _userRepo.ExistsAsync(userId))
                throw new EntityNotFoundException("User does not exist.");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                    return (false, "Only image files (.jpg, .jpeg, .png, .gif) are allowed.", null, null);

                if (file.Length > 10_000_000)
                    return (false, "File size cannot exceed 10MB.", null, null);

                var photoUrl = await _storage.UploadFileAsync(file, folder: "uploads");
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
                var submissions = await _photoRepo.GetSubmissionsForTaskAsync(taskId);
                await PopulateUserNamesAsync(submissions);
                return submissions;
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
                var submissions = await _photoRepo.GetSubmissionsByUserAsync(userId);
                await PopulateUserNamesAsync(submissions);
                return submissions;
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

                await _storage.DeleteFileAsync(submission.PhotoUrl);

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
                var submissions = await _photoRepo.GetSubmissionsForChallengeAsync(challengeId);
                await PopulateUserNamesAsync(submissions);
                return submissions;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching submissions for challenge {challengeId}: {ex.Message}");
            }
        }

        private async Task PopulateUserNamesAsync(List<PhotoSubmission> submissions)
        {
            if (submissions == null || submissions.Count == 0)
                return;

            var userIds = submissions.Select(s => s.UserId);
            var names = await _userRepo.GetUserNamesAsync(userIds);

            foreach (var submission in submissions)
            {
                if (names.TryGetValue(submission.UserId, out var name))
                {
                    submission.UserName = name;
                }
            }
        }
    }
}