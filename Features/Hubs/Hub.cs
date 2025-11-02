using System.Text.Json.Serialization;

namespace PhotoScavengerHunt.Features.Hubs;

public class Hub
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string JoinCode { get; set; } = "";  // Unique code to join (e.g., "ABC123")
    public int CreatorId { get; set; }  // User who created the hub
    public DateTime CreatedAt { get; set; }
    public bool IsPrivate { get; set; } = false;  // Public hubs appear in list, private need join code

    // Navigation properties - ignore when null or empty
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<HubMember>? Members { get; set; }
}

