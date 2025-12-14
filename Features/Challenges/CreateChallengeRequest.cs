namespace PhotoScavengerHunt.Features.Challenges;

public record CreateChallengeRequest
{
    public string Name { get; init; } = string.Empty;
    public int CreatorId { get; init; }
    public IEnumerable<int> TaskIds { get; init; } = Enumerable.Empty<int>();
    public DateTime? Deadline { get; init; }
    public bool IsPrivate { get; init; }
    public int? MaxParticipants { get; init; }

    public TimeSpan? SubmissionDuration { get; init; }
    public TimeSpan? VotingDuration { get; init; }

    // New canonical constructor (multiple task ids)
    public CreateChallengeRequest(string name, int creatorId, IEnumerable<int> taskIds, DateTime? deadline, bool isPrivate = false, int? maxParticipants = null, TimeSpan? submissionDuration = null, TimeSpan? votingDuration = null)
    {
        Name = name;
        CreatorId = creatorId;
        TaskIds = taskIds ?? Enumerable.Empty<int>();
        Deadline = deadline;
        IsPrivate = isPrivate;
        MaxParticipants = maxParticipants;
        SubmissionDuration = submissionDuration;
        VotingDuration = votingDuration;
    }
}