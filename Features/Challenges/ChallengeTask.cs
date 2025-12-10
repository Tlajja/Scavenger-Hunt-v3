using System.Text.Json.Serialization;
using PhotoScavengerHunt.Features.Tasks;
namespace PhotoScavengerHunt.Features.Challenges;

public class ChallengeTask
{
 public int ChallengeId { get; set; }
 public int TaskId { get; set; }

 [JsonIgnore]
 public Challenge? Challenge { get; set; }
 public HuntTask? Task { get; set; }
 
 public DateTime? Deadline { get; set; }
}