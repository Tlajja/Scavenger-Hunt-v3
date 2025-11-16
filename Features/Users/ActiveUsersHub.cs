using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace PhotoScavengerHunt.Features.Users
{
    public class ActiveUsersHub : Hub
    {
        private static readonly ConcurrentDictionary<string, DateTime> ActiveUsers = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                ActiveUsers[userId] = DateTime.UtcNow;

                await Clients.All.SendAsync(
                    "ActiveUsersCountUpdated",
                    ActiveUsers.Count
                );
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                ActiveUsers.TryRemove(userId, out _);

                await Clients.All.SendAsync(
                    "ActiveUsersCountUpdated",
                    ActiveUsers.Count
                );
            }

            await base.OnDisconnectedAsync(ex);
        }
    }
}