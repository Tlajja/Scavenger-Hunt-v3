using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace PhotoScavengerHunt.Features.Users
{
 // Custom provider to tell SignalR what a "user" is for this app
 // Priority: NameIdentifier/sub claim -> query string `userId` -> query string `username`
 public class CustomUserIdProvider : IUserIdProvider
 {
 public string? GetUserId(HubConnectionContext connection)
 {
 var http = connection.GetHttpContext();
 var user = connection.User;

 //1) Try standard claims (when authentication is enabled)
 var fromClaims = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
 ?? user?.FindFirst("sub")?.Value
 ?? user?.Identity?.Name;

 if (!string.IsNullOrWhiteSpace(fromClaims))
 {
 return fromClaims;
 }

 //2) Fallback to query string provided by the client
 var userId = http?.Request.Query["userId"].FirstOrDefault();
 if (!string.IsNullOrWhiteSpace(userId))
 {
 return userId;
 }

 var username = http?.Request.Query["username"].FirstOrDefault();
 if (!string.IsNullOrWhiteSpace(username))
 {
 return username;
 }

 return null;
 }
 }
}