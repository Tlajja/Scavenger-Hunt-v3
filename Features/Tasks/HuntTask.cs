using System;

namespace PhotoScavengerHunt.Features.Tasks;

public class HuntTask
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public DateTime Deadline { get; set; }
    public HuntTaskStatus Status { get; set; }
    public int AuthorId { get; set; }

    public static HuntTask Create(string description, DateTime? deadline = null, HuntTaskStatus status = HuntTaskStatus.Open)
    {
        return new HuntTask
        {
            Description = description,
            Deadline = deadline ?? DateTime.UtcNow.AddDays(7), // If no deadline is provided, set it to 7 days from now
            Status = status
        };
    }
}