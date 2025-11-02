namespace PhotoScavengerHunt.Features.Hubs;

public class CreateHubRequest
{
    public string Name { get; set; } = "";
    public int CreatorId { get; set; }
    public bool IsPrivate { get; set; } = false;
}

