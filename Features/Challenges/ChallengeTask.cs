using System.Text.Json.Serialization;
namespace PhotoScavengerHunt.Features.Challenges;

public class ChallengeTask
{
 public int ChallengeId { get; set; }
 public int TaskId { get; set; }

 [JsonIgnore]
 public Challenge? Challenge { get; set; }
 public BasicTask? Task { get; set; }
}