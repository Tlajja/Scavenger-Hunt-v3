using PhotoScavengerHunt.Features.Challenges.Abstractions;
using PhotoScavengerHunt.Interfaces;

namespace PhotoScavengerHunt.Features.Challenges;

public class Challenge : IHasCreatedAt, IHasDeadline
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int CreatorId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? Deadline { get; set; }
    public DateTime? SubmissionEndsAt { get; set; }
    public DateTime? VotingEndsAt { get; set; }

    public bool IsPrivate { get; set; }
    public string JoinCode { get; set; } = "";
    public ChallengeStatus Status { get; set; } = ChallengeStatus.Open;
    public int? MaxParticipants { get; set; }

    public int? WinnerId { get; set; }
    public List<ChallengeParticipant>? Participants { get; set; }
    public List<ChallengeTask> ChallengeTasks { get; set; } = new();
}