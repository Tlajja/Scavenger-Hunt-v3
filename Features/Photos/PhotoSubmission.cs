namespace PhotoScavengerHunt.Features.Photos
{
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
}