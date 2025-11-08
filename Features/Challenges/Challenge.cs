using System.Text.Json.Serialization;

namespace PhotoScavengerHunt.Features.Challenges;

public class Challenge
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int TaskId { get; set; }              // susietas su HuntTask
    public int CreatorId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime Deadline { get; set; }       // challenge trukmë
    public bool IsPrivate { get; set; } = false; // vieðas ar privatus

    // navigacija
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ChallengeParticipant>? Participants { get; set; }
}
