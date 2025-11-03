namespace PhotoScavengerHunt.Features.Hubs;

public record CreateHubRequest(string Name, int CreatorId, bool IsPrivate = false);

