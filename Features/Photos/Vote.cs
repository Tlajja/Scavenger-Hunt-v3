
using System.Text.Json.Serialization;
using PhotoScavengerHunt.Features.Users;

namespace PhotoScavengerHunt.Features.Photos
{
    public class Vote
    {
        public int Id { get; set; }
        public int PhotoSubmissionId { get; set; }
        public int UserId { get; set; }
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public PhotoSubmission? PhotoSubmission { get; set; }
        
        [JsonIgnore]
        public UserProfile? User { get; set; }
    }
}