using PhotoScavengerHunt.Features.Challenges.Abstractions;
using PhotoScavengerHunt.Interfaces;

public class HuntTask : IHasTimeMetadata
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public int AuthorId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? Deadline { get; set; }
}