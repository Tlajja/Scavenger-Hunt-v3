using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Features.Challenges.Abstractions;
using PhotoScavengerHunt.Interfaces;

public static class SortingExtensions
{
    public static IQueryable<T> SortBy<T>(this IQueryable<T> source, ChallengeSortBy sortBy)
        where T : IHasCreatedAt, IHasDeadline
    {
        return sortBy switch
        {
            ChallengeSortBy.CreatedAtAsc => source.OrderBy(x => x.CreatedAt),
            ChallengeSortBy.CreatedAtDesc => source.OrderByDescending(x => x.CreatedAt),

            ChallengeSortBy.DeadlineAsc =>
                source.OrderBy(x => !x.HasDeadline).ThenBy(x => x.Deadline),

            ChallengeSortBy.DeadlineDesc =>
                source.OrderByDescending(x => x.HasDeadline)
                      .ThenByDescending(x => x.Deadline),

            _ => source.OrderByDescending(x => x.CreatedAt),
        };
    }
}