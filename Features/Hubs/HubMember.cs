using System.Text.Json.Serialization;
using PhotoScavengerHunt.Features.Users;

namespace PhotoScavengerHunt.Features.Hubs;

public class HubMember
{
    public int Id { get; set; }
    public int HubId { get; set; }
    public int UserId { get; set; }
    public HubMemberRole Role { get; set; } = HubMemberRole.Member;
    public DateTime JoinedAt { get; set; }

    // Navigation properties - always ignore
    [JsonIgnore]
    public Hub? Hub { get; set; }
    
    [JsonIgnore]
    public UserProfile? User { get; set; }
}

