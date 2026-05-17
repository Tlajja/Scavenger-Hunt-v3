namespace PhotoScavengerHunt.Features.Challenges;

public class ChallengeMapDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string JoinCode { get; set; } = "";
    public int ParticipantCount { get; set; }
    public int MaxParticipants { get; set; }
    public Location Location { get; set; } = new();
}
