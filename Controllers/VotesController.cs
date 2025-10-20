using Microsoft.AspNetCore.Mvc;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VotesController : ControllerBase
    {
        private readonly PhotoScavengerHuntDbContext _db;

        public VotesController(PhotoScavengerHuntDbContext db)
        {
            _db = db;
        }

        [HttpPost("{submissionId}")]
        public async Task<IActionResult> UpvotePhoto(int submissionId)
        {
            var submission = await _db.Photos.FindAsync(submissionId);
            if (submission == null)
                return NotFound("Submission not found.");

            submission.Votes += 1;
            await _db.SaveChangesAsync();

            return Ok(submission);
        }
    }
}