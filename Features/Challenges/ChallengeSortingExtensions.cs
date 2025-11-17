using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Interfaces;

public static class ChallengeSortingExtensions
{
    public static TSource SortBy<TSource, TItem>(
        this TSource source,
        ChallengeSortBy sortBy
    )
        where TSource : IEnumerable<TItem>   // Generic constraint #1 (collection)
        where TItem : IHasTimeMetadata       // Generic constraint #2 (item metadata)
    {
        // If IQueryable<T>, use IQueryable sorting (deferred, EF Core safe)
        if (source is IQueryable<TItem> queryable)
        {
            return (TSource)(sortBy switch
            {
                ChallengeSortBy.CreatedAtAsc => queryable.OrderBy(x => x.CreatedAt),
                ChallengeSortBy.CreatedAtDesc => queryable.OrderByDescending(x => x.CreatedAt),
                ChallengeSortBy.DeadlineAsc => queryable.OrderBy(x => x.Deadline == null).ThenBy(x => x.Deadline),
                ChallengeSortBy.DeadlineDesc => queryable.OrderByDescending(x => x.Deadline ?? DateTime.MaxValue),
                _ => queryable.OrderByDescending(x => x.CreatedAt)
            });
        }

        // Otherwise use IEnumerable<T>
        IEnumerable<TItem> enumerable = source;

        return (TSource)(sortBy switch
        {
            ChallengeSortBy.CreatedAtAsc => enumerable.OrderBy(x => x.CreatedAt),
            ChallengeSortBy.CreatedAtDesc => enumerable.OrderByDescending(x => x.CreatedAt),
            ChallengeSortBy.DeadlineAsc => enumerable.OrderBy(x => x.Deadline == null).ThenBy(x => x.Deadline),
            ChallengeSortBy.DeadlineDesc => enumerable.OrderByDescending(x => x.Deadline ?? DateTime.MaxValue),
            _ => enumerable.OrderByDescending(x => x.CreatedAt)
        });
    }
}