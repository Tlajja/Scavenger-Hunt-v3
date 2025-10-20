using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotoSubmissionsController : ControllerBase
    {
        private readonly PhotoScavengerHuntDbContext _db;
        private readonly IWebHostEnvironment _env;

        public PhotoSubmissionsController(
            PhotoScavengerHuntDbContext db,
            IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadPhoto([FromForm] int taskId, [FromForm] int userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Only image files (.jpg, .jpeg, .png, .gif) are allowed.");
            }

            if (file.Length > 10_000_000)
            {
                return BadRequest("File size cannot exceed 10MB.");
            }

            if (!await _db.Tasks.AnyAsync(t => t.Id == taskId))
            {
                return BadRequest("Task does not exist.");
            }

            if (!await _db.Users.AnyAsync(u => u.Id == userId))
            {
                return BadRequest("User does not exist.");
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
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

            _db.Photos.Add(submission);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Photo uploaded successfully.",
                photoUrl = photoUrl,
                submissionId = submission.Id
            });
        }

        [HttpGet("task/{taskId}")]
        public async Task<IEnumerable<PhotoSubmission>> GetSubmissionsForTask(int taskId) =>
            await _db.Photos
                .Include(p => p.Comments)
                .Where(s => s.TaskId == taskId)
                .ToListAsync();

        [HttpGet("user/{userId}")]
        public async Task<IEnumerable<PhotoSubmission>> GetSubmissionsByUser(int userId) =>
            await _db.Photos
                .Include(p => p.Comments)
                .Where(s => s.UserId == userId)
                .ToListAsync();
    }
}