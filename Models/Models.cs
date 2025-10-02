namespace PhotoScavengerHunt.Models
{
    public class HuntTask
    {
        public int Id { get; set; }
        public string Description { get; set; } = "";
        public DateTime Deadline { get; set; }
        public HuntTaskStatus Status { get; set; }
    }

    public record CreateTaskRequest(string Description, DateTime Deadline);

    public class PhotoSubmission
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string PhotoUrl { get; set; } = "";
        public int Votes { get; set; }

        public List<Comment> Comments { get; set; } = new();

        public PhotoSubmission() { }

        public PhotoSubmission(int taskId, int userId, string photoUrl)
        {
            TaskId = taskId;
            UserId = userId;
            PhotoUrl = photoUrl;
            Votes = 0;
        }
    }

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

    public class Comment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Text { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}