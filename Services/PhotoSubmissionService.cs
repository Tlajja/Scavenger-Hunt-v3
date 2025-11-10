using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Services
{
    public class PhotoSubmissionService
    {
        private readonly PhotoScavengerHuntDbContext _dbContext;
        private readonly IWebHostEnvironment _env;

        public PhotoSubmissionService(PhotoScavengerHuntDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
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
                    var challenge = await _dbContext.Challenges.FirstOrDefaultAsync(c => c.Id == challengeId.Value);
                    if (challenge == null)
                        return (false, "Challenge not found.", null, null);
                    taskId = challenge.TaskId;
                }

                if (!taskId.HasValue || !await _dbContext.Tasks.AnyAsync(t => t.Id == taskId.Value))
                    return (false, "Task does not exist.", null, null);

                if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
                    return (false, "User does not exist.", null, null);

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

                _dbContext.Photos.Add(submission);
                await _dbContext.SaveChangesAsync();

                return (true, "Photo uploaded successfully.", photoUrl, submission.Id);
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
                return await _dbContext.Photos
                    .Include(p => p.Comments)
                    .Where(s => s.TaskId == taskId)
                    .ToListAsync();
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
                return await _dbContext.Photos
                    .Include(p => p.Comments)
                    .Where(s => s.UserId == userId)
                    .ToListAsync();
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
                var submission = await _dbContext.Photos.FindAsync(submissionId);
                if (submission == null)
                    return (false, "Submission not found.");

                // Delete file from server (if it exists)
                var filePath = Path.Combine(_env.WebRootPath, submission.PhotoUrl.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _dbContext.Photos.Remove(submission);
                await _dbContext.SaveChangesAsync();

                return (true, "Submission deleted successfully.");
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
                return await _dbContext.Photos
                    .Include(p => p.Comments)
                    .Where(p => p.ChallengeId == challengeId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching submissions for challenge {challengeId}: {ex.Message}");
            }
        }
    }
}