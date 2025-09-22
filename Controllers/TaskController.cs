using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Models;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private static readonly List<HuntTask> tasks = new()
        {
            new HuntTask(1, "Red car", DateTime.Parse("2025-10-01"), HuntTaskStatus.Open),
            new HuntTask(2, "Blue mailbox", DateTime.Parse("2025-10-02"), HuntTaskStatus.Open),
            new HuntTask(3, "Park bench", DateTime.Parse("2025-10-03"), HuntTaskStatus.Open)
        };

        private static int nextTaskId = tasks.Count + 1;

        [HttpPost]
        public IActionResult CreateTask(CreateTaskRequest req)
        {
            var task = new HuntTask(nextTaskId++, req.Description, req.Deadline, HuntTaskStatus.Open);
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
