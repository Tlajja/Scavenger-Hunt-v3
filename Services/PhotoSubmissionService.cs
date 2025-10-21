using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Services
{
    public class PhotoSubmissionService
    {
        private readonly PhotoScavengerHuntDbContext dbContext;
        private readonly IWebHostEnvironment _env;

        public PhotoSubmissionService(PhotoScavengerHuntDbContext dbContext, IWebHostEnvironment env)
        {
            this.dbContext = dbContext;
            _env = env;
        }

        public async Task<(bool Success, string Message, string? PhotoUrl, int? SubmissionId)> UploadPhotoAsync(int taskId, int userId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return (false, "No file uploaded.", null, null);

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                    return (false, "Only image files (.jpg, .jpeg, .png, .gif) are allowed.", null, null);

                if (file.Length > 10_000_000)
                    return (false, "File size cannot exceed 10MB.", null, null);

                if (!await dbContext.Tasks.AnyAsync(t => t.Id == taskId))
                    return (false, "Task does not exist.", null, null);

                if (!await dbContext.Users.AnyAsync(u => u.Id == userId))
                    return (false, "User does not exist.", null, null);

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
                    TaskId = taskId,
                    UserId = userId,
                    PhotoUrl = photoUrl,
                    Votes = 0,
                    Comments = new List<Comment>()
                };

                dbContext.Photos.Add(submission);
                await dbContext.SaveChangesAsync();

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
                return await dbContext.Photos
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
                return await dbContext.Photos
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
                var submission = await dbContext.Photos.FindAsync(submissionId);
                if (submission == null)
                    return (false, "Submission not found.");

                // Ištrinam failą iš serverio, jei jis egzistuoja
                var filePath = Path.Combine(_env.WebRootPath, submission.PhotoUrl.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                dbContext.Photos.Remove(submission);
                await dbContext.SaveChangesAsync();

                return (true, "Submission deleted successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting submission: {ex.Message}");
            }
        }
    }
}