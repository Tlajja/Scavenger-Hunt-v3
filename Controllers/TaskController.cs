using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Models;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private static readonly List<HuntTask> tasks = new();
        private static int nextTaskId = 1;

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