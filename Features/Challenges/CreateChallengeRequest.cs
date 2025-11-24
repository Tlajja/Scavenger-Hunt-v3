namespace PhotoScavengerHunt.Features.Challenges;

public record CreateChallengeRequest
{
    public string Name { get; init; } = string.Empty;
    public int CreatorId { get; init; }
    public IEnumerable<int> TaskIds { get; init; } = Enumerable.Empty<int>();
    public DateTime? Deadline { get; init; }
    public bool IsPrivate { get; init; }

    // New canonical constructor (multiple task ids)
    public CreateChallengeRequest(string name, int creatorId, IEnumerable<int> taskIds, DateTime? deadline, bool isPrivate = false)
    {
        Name = name;
        CreatorId = creatorId;
        TaskIds = taskIds ?? Enumerable.Empty<int>();
        Deadline = deadline;
        IsPrivate = isPrivate;
    }
}