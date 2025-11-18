using Microsoft.AspNetCore.Mvc;
using PhotoScavengerHunt.Features.Tasks;
using PhotoScavengerHunt.Services.Interfaces;

namespace PhotoScavengerHunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _taskLogger;

        public TasksController(ITaskService taskService, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _taskLogger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(CreateTaskRequest req)
        {
            try
            {
                var task = await _taskService.CreateTaskAsync(req);
                return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _taskLogger.LogError(ex, "Internal error during task creation.");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("user")]
        public async Task<IActionResult> CreateUserTask(CreateTaskRequest req)
        {
            try
            {
                var task = await _taskService.CreateUserTaskAsync(req);
                return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _taskLogger.LogError(ex, "Internal error during user task creation.");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            try
            {
                var tasks = await _taskService.GetTasksAsync();
                return Ok(tasks);
            }
            catch (InvalidOperationException ex)
            {
                _taskLogger.LogError(ex, "Error getting tasks.");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            try
            {
                var task = await _taskService.GetTaskByIdAsync(id);
                return task is null ? NotFound() : Ok(task);
            }
            catch (InvalidOperationException ex)
            {
                _taskLogger.LogError(ex, "Error fetching task by ID {TaskId}.", id);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("user/{userId}/{taskId}")]
        public async Task<IActionResult> DeleteUserTask(int userId, int taskId)
        {
            try
            {
                await _taskService.DeleteUserTaskAsync(userId, taskId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _taskLogger.LogError(ex, "Error deleting task {TaskId}.", taskId);
                return StatusCode(500, ex.Message);
            }
        }
    }
}