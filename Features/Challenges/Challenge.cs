using System.Text.Json.Serialization;

namespace PhotoScavengerHunt.Features.Challenges;

public class Challenge
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int TaskId { get; set; }      
    public int CreatorId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? Deadline { get; set; }       // challenge end time
    public bool IsPrivate { get; set; } = false;
    public string JoinCode { get; set; } = "";
    public ChallengeStatus Status { get; set; } = ChallengeStatus.Open;

    public int? WinnerId {get; set; }

    [JsonPropertyName("members")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ChallengeParticipant>? Participants { get; set; }
}
