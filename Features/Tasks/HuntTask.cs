namespace PhotoScavengerHunt.Features.Tasks;

public class HuntTask
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public DateTime Deadline { get; set; }
    public HuntTaskStatus Status { get; set; }
    public int AuthorId { get; set; }
}