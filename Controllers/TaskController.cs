using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Features.Tasks;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private static readonly List<HuntTask> tasks = new()
        {
            new HuntTask { Id = 1, Description = "Red car", Deadline = DateTime.Parse("2025-10-01"), Status = HuntTaskStatus.Open },
            new HuntTask { Id = 2, Description = "Blue mailbox", Deadline = DateTime.Parse("2025-10-02"), Status = HuntTaskStatus.Open },
            new HuntTask { Id = 3, Description = "Park bench", Deadline = DateTime.Parse("2025-10-03"), Status = HuntTaskStatus.Open }
        };

        private static int nextTaskId = tasks.Count + 1;

        [HttpPost]
        public IActionResult CreateTask(CreateTaskRequest req)
        {
            var task = new HuntTask
            {
                Id = nextTaskId++,
                Description = req.Description,
                Deadline = req.Deadline,
                Status = HuntTaskStatus.Open
            };

            tasks.Add(task);
            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
        }

        [HttpGet]
        public IEnumerable<HuntTask> GetTasks() => tasks;

        [HttpGet("{id}")]
        public IActionResult GetTaskById(int id)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            return task is null ? NotFound() : Ok(task);
        }
    }
}
