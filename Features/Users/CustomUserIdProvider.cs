using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace PhotoScavengerHunt.Features.Users
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var http = connection.GetHttpContext();
            var user = connection.User;

            var fromClaims = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? user?.FindFirst("sub")?.Value
                             ?? user?.Identity?.Name;

            if (!string.IsNullOrWhiteSpace(fromClaims))
            {
                return fromClaims;
            }

            var userId = http?.Request.Query["userId"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(userId))
            {
                return userId;
            }

            var username = http?.Request.Query["username"].FirstOrDefault();
            return !string.IsNullOrWhiteSpace(username) ? username : null;
        }
    }
}