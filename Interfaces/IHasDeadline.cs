namespace PhotoScavengerHunt.Features.Challenges.Abstractions;

public interface IHasDeadline
{
    DateTime? Deadline { get; }

    bool HasDeadline => Deadline != null;

    bool IsExpired => Deadline != null && Deadline < DateTime.UtcNow;

    bool IsDeadlineWithin(TimeSpan max)
        => Deadline != null && Deadline <= DateTime.UtcNow.Add(max);
}