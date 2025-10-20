// Features/Photos/PhotoSubmission.cs
namespace PhotoScavengerHunt.Features.Photos
{
    public class PhotoSubmission
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
        public int Votes { get; set; }
        public List<Comment> Comments { get; set; } = new();
    }
}