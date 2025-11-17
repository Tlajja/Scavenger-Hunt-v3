using PhotoScavengerHunt.Features.Challenges.Abstractions;

namespace PhotoScavengerHunt.Interfaces
{
 // Combines time-related interfaces into a single constraint for convenience
 public interface IHasTimeMetadata : IHasCreatedAt, IHasDeadline
 {
 // Optional shared behavior
 bool HasExpired() => Deadline.HasValue && Deadline.Value < DateTime.UtcNow;
 }
}