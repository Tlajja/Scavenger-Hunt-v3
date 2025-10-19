namespace PhotoScavengerHunt.Features.Tasks
{
    public record CreateTaskRequest(string Description, DateTime? Deadline, int AuthorId);
}