using System.Text.Json.Serialization;
using PhotoScavengerHunt.Features.Users;

namespace PhotoScavengerHunt.Features.Challenges;

public class ChallengeParticipant
{
    public int Id { get; set; }
    public int ChallengeId { get; set; }
    public int UserId { get; set; }
    public ChallengeRole Role { get; set; } = ChallengeRole.Participant;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public Challenge? Challenge { get; set; }

    [JsonIgnore]
    public UserProfile? User { get; set; }
}