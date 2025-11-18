namespace PhotoScavengerHunt.Interfaces
{
    public interface IHasCreatedAt
    {
        DateTime CreatedAt { get; }

        TimeSpan Age => DateTime.UtcNow - CreatedAt;

        bool IsCreatedBefore(DateTime other)
            => CreatedAt < other;
    }
}