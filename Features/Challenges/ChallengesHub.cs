using Microsoft.AspNetCore.SignalR;

namespace PhotoScavengerHunt.Features.Challenges;

public class ChallengesHub : Hub
{
    // Broadcast when a new challenge is created
    public static async Task NotifyChallengeCreated(IHubContext<ChallengesHub> hubContext, Challenge challenge)
    {
        await hubContext.Clients.All.SendAsync("ChallengeCreated", new
        {
            challenge.Id,
            challenge.Name,
            challenge.IsPrivate,
            challenge.MaxParticipants,
            challenge.Latitude,
            challenge.Longitude,
            challenge.LocationName,
            challenge.JoinCode,
            challenge.Deadline,
            ParticipantCount = 0
        });
    }

    // Broadcast when a challenge is updated (participant joined, etc.)
    public static async Task NotifyChallengeUpdated(IHubContext<ChallengesHub> hubContext, int challengeId, int participantCount, int maxParticipants)
    {
        await hubContext.Clients.All.SendAsync("ChallengeUpdated", new
        {
            Id = challengeId,
            ParticipantCount = participantCount,
            MaxParticipants = maxParticipants
        });
    }

    // Broadcast when a challenge is deleted
    public static async Task NotifyChallengeDeleted(IHubContext<ChallengesHub> hubContext, int challengeId)
    {
        await hubContext.Clients.All.SendAsync("ChallengeDeleted", challengeId);
    }
}
