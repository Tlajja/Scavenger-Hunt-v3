using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Tasks;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly PhotoScavengerHuntDbContext _db;

        public TasksController(PhotoScavengerHuntDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(CreateTaskRequest req)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(req.Description))
            {
                return BadRequest("Task description cannot be empty.");
            }
            if (req.Deadline <= DateTime.UtcNow)
            {
                return BadRequest("Deadline cannot be in the past.");
            }

            var task = new HuntTask
            {
                Description = req.Description,
                Deadline = req.Deadline,
                Status = HuntTaskStatus.Open
            };

            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
        }

        [HttpGet]
        public async Task<IEnumerable<HuntTask>> GetTasks() =>
            await _db.Tasks.ToListAsync();

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _db.Tasks.FindAsync(id);
            return task is null ? NotFound() : Ok(task);
        }
    }
}
