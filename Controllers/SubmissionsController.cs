using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Models;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionsController : ControllerBase
    {
        private static readonly List<PhotoSubmission> submissions = new();
        private static readonly List<HuntTask> tasks = new();
        private static int nextSubmissionId = 1;

        [HttpPost]
        public IActionResult SubmitPhoto(int taskId, string userName, string photoUrl)
        {
            if (!tasks.Any(t => t.Id == taskId))
                return NotFound("Task not found");

            var submission = new PhotoSubmission(
                Id: nextSubmissionId++,
                TaskId: taskId,
                UserName: userName,
                PhotoUrl: photoUrl,
                Votes: 0
            );

            submissions.Add(submission);
            return CreatedAtAction(nameof(GetSubmissionsForTask), new { taskId }, submission);
        }

        [HttpGet("{taskId}")]
        public IEnumerable<PhotoSubmission> GetSubmissionsForTask(int taskId) =>
            submissions.Where(s => s.TaskId == taskId);

        [HttpPost("{id}/vote")]
        public IActionResult UpvotePhoto(int id)
        {
            var submission = submissions.FirstOrDefault(s => s.Id == id);
            if (submission == null) return NotFound();

            var updated = submission with { Votes = submission.Votes + 1 };
            submissions.Remove(submission);
            submissions.Add(updated);

            return Ok(updated);
        }
    }
}