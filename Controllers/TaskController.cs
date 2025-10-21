using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Features.Tasks;
using PhotoScavengerHunt.Services;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _service;
        private readonly ILogger<TasksController> _logger;

        public TasksController(TaskService service, ILogger<TasksController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(CreateTaskRequest req)
        {
            try
            {
                var task = await _service.CreateTaskAsync(req);
                return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Internal error during task creation.");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("user")]
        public async Task<IActionResult> CreateUserTask(CreateTaskRequest req)
        {
            try
            {
                var task = await _service.CreateUserTaskAsync(req);
                return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Internal error during user task creation.");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            try
            {
                var tasks = await _service.GetTasksAsync();
                return Ok(tasks);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error getting tasks.");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            try
            {
                var task = await _service.GetTaskByIdAsync(id);
                return task is null ? NotFound() : Ok(task);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error fetching task by ID {TaskId}.", id);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("user/{userId}/{taskId}")]
        public async Task<IActionResult> DeleteUserTask(int userId, int taskId)
        {
            try
            {
                await _service.DeleteUserTaskAsync(userId, taskId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}.", taskId);
                return StatusCode(500, ex.Message);
            }
        }
    }
}