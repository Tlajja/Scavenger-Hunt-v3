using PhotoScavengerHunt.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var tasks = new List<HuntTask>();
var submissions = new List<PhotoSubmission>();
var users = new List<UserProfile>();
var nextTaskId = 1;
var nextSubmissionId = 1;
var nextUserId = 1;

// Create a new task
app.MapPost("/tasks", (CreateTaskRequest req) =>
{
    var task = new HuntTask(nextTaskId++, req.Description, req.Deadline, HuntTaskStatus.Open);
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

    var submission = new PhotoSubmission(
        Id: nextSubmissionId++,
        TaskId: taskId,
        UserName: userName,
        PhotoUrl: photoUrl,
        Votes: 0
    );
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

// Create a user profile
app.MapPost("/users", (string name, int age) =>
{
    var profile = new UserProfile
    {
        Id = nextUserId++,
        Name = name,
        Age = age,
    };
    users.Add(profile);
    return Results.Created($"/users/{profile.Id}", profile);
});

// Get all users
app.MapGet("/users", () => users);

app.Run();