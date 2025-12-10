namespace PhotoScavengerHunt.Features.Tasks
{
    public record CreateTaskRequest(string Description, int AuthorId, System.DateTime? Deadline = null, int? TimerSeconds = null);
}