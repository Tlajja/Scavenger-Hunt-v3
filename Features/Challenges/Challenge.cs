using PhotoScavengerHunt.Features.Challenges.Abstractions;
using PhotoScavengerHunt.Interfaces;

namespace PhotoScavengerHunt.Features.Challenges;

public class Challenge : IHasCreatedAt, IHasDeadline
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int TaskId { get; set; }
    public int CreatorId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? Deadline { get; set; }

    public bool IsPrivate { get; set; }
    public string JoinCode { get; set; } = "";
    public ChallengeStatus Status { get; set; } = ChallengeStatus.Open;

    public int? WinnerId { get; set; }
    public List<ChallengeParticipant>? Participants { get; set; }
}