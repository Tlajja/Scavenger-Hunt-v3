namespace PhotoScavengerHunt.Models
{
    public record HuntTask(int Id, string Description, DateTime Deadline, HuntTaskStatus Status);
    public record CreateTaskRequest(string Description, DateTime Deadline);
    public record PhotoSubmission(
        int Id,
        int TaskId,
        int UserId,
        string PhotoUrl,
        int Votes,
        List<Comment> Comments // Stores user-linked comments
    );

    public enum HuntTaskStatus
    {
        Open,
        Closed,
        Completed
    }

    public class UserProfile
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    public record Comment(int UserId, string Text, DateTime Timestamp);
}