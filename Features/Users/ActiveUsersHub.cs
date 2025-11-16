using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace PhotoScavengerHunt.Features.Users
{
    public class ActiveUsersHub : Hub
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> UserConnections = new();

        private Task BroadcastCountIfChanged(bool changed)
        {
            if (!changed) return Task.CompletedTask;
            var distinctUsers = UserConnections.Count;
            return Clients.All.SendAsync("ActiveUsersCountUpdated", distinctUsers);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var connections = UserConnections.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());
                connections[Context.ConnectionId] = 0;

                var changed = connections.Count == 1;
                await BroadcastCountIfChanged(changed);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                if (UserConnections.TryGetValue(userId, out var connections))
                {
                    connections.TryRemove(Context.ConnectionId, out _);

                    bool changed = false;
                    if (connections.IsEmpty)
                    {
                        UserConnections.TryRemove(userId, out _);
                        changed = true;
                    }

                    await BroadcastCountIfChanged(changed);
                }
            }

            await base.OnDisconnectedAsync(ex);
        }
    }
}