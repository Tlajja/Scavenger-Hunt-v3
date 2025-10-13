using System;

namespace PhotoScavengerHunt.Features.Tasks;

public class HuntTask
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public DateTime Deadline { get; set; }
    public HuntTaskStatus Status { get; set; }
    public int AuthorId { get; set; }

    // Optional arguments metodas su numatytosiomis reikšmėmis
    public static HuntTask Create(string description, DateTime? deadline = null, HuntTaskStatus status = HuntTaskStatus.Open)
    {
        return new HuntTask
        {
            Description = description,
            Deadline = deadline ?? DateTime.UtcNow.AddDays(7), // jei deadline nenurodytas, pridėti 7 dienas
            Status = status
        };
    }
}