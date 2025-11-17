using Microsoft.AspNetCore.SignalR;
using PhotoScavengerHunt.Services;

public class ActiveUsersHub(ActiveUsersService activeUsers) : Hub
{
    private readonly ActiveUsersService _activeUsers = activeUsers;

    private Task BroadcastIfChanged(int newCount)
    {
        if (newCount < 0)
            return Task.CompletedTask;

        return Clients.All.SendAsync("ActiveUsersCountUpdated", newCount);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            var newCount = _activeUsers.AddConnection(userId, Context.ConnectionId);
            if (newCount >= 0)
                await Clients.All.SendAsync("ActiveUsersCountUpdated", newCount);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            var newCount = _activeUsers.RemoveConnection(userId, Context.ConnectionId);
            await BroadcastIfChanged(newCount);
        }

        await base.OnDisconnectedAsync(ex);
    }
}