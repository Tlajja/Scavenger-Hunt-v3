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

    public virtual bool Equals(CreateChallengeRequest? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;
        if (!string.Equals(Name, other.Name, StringComparison.Ordinal)) return false;
        if (CreatorId != other.CreatorId) return false;
        if (IsPrivate != other.IsPrivate) return false;
        if (Deadline != other.Deadline) return false;
        return (TaskIds ?? Enumerable.Empty<int>()).SequenceEqual(other.TaskIds ?? Enumerable.Empty<int>());
    }

    public override int GetHashCode()
    {
        var hc = new HashCode();
        hc.Add(Name, StringComparer.Ordinal);
        hc.Add(CreatorId);
        foreach (var id in TaskIds ?? Enumerable.Empty<int>()) hc.Add(id);
        hc.Add(Deadline);
        hc.Add(IsPrivate);
        return hc.ToHashCode();
    }
}