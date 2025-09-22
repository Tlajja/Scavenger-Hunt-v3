namespace PhotoScavengerHunt.Models
{
    public record HuntTask(int Id, string Description, DateTime Deadline, HuntTaskStatus Status);
    public record CreateTaskRequest(string Description, DateTime Deadline);
    public record PhotoSubmission(int Id, int TaskId, string UserName, string PhotoUrl, int Votes);

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
}