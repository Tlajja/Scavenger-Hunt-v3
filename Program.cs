var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// In-memory storage (temporary - instead of a database)
var tasks = new List<HuntTask>();
var submissions = new List<PhotoSubmission>();
var nextTaskId = 1;
var nextSubmissionId = 1;

// Create a new task
app.MapPost("/tasks", (CreateTaskRequest req) =>
{
    var task = new HuntTask(nextTaskId++, req.Description, req.Deadline);
    tasks.Add(task);
    return Results.Created($"/tasks/{task.Id}", task);
});

// Get all tasks
app.MapGet("/tasks", () => tasks);

// Submit a photo for a task
app.MapPost("/submissions", (int taskId, string userName, string photoUrl) =>
{
    if (!tasks.Any(t => t.Id == taskId))
        return Results.NotFound("Task not found");

    var submission = new PhotoSubmission(nextSubmissionId++, taskId, userName, photoUrl, 0);
    submissions.Add(submission);
    return Results.Created($"/submissions/{submission.Id}", submission);
});

// Get all submissions for a task
app.MapGet("/submissions/{taskId}", (int taskId) =>
{
    var taskSubmissions = submissions.Where(s => s.TaskId == taskId);
    return taskSubmissions;
});

// Upvote a photo
app.MapPost("/submissions/{id}/vote", (int id) =>
{
    var submission = submissions.FirstOrDefault(s => s.Id == id);
    if (submission == null) return Results.NotFound();

    var updated = submission with { Votes = submission.Votes + 1 };
    submissions.Remove(submission);
    submissions.Add(updated);

    return Results.Ok(updated);
});

app.Run();

record HuntTask(int Id, string Description, DateTime Deadline);

record CreateTaskRequest(string Description, DateTime Deadline);

record PhotoSubmission(int Id, int TaskId, string UserName, string PhotoUrl, int Votes);
