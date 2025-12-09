using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PhotoScavengerHunt.Features.Photos
{
    public class Comment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Text { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; } = "";

        public int PhotoSubmissionId { get; set; }

        [JsonIgnore]
        public PhotoSubmission? PhotoSubmission { get; set; }

        [NotMapped]
        public bool IsRecent => Timestamp > DateTime.UtcNow.AddHours(-24);

        [NotMapped]
        public string Preview => Text.Length > 50 ? Text[..50] + "..." : Text;
    }
}