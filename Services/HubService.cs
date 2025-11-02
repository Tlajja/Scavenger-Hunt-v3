using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Hubs;

namespace PhotoScavengerHunt.Services
{
    public class HubService
    {
        private readonly PhotoScavengerHuntDbContext dbContext;

        public HubService(PhotoScavengerHuntDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private string GenerateJoinCode()
        {
            // Generate a random 6-character alphanumeric code
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<string> GenerateUniqueJoinCodeAsync()
        {
            string code;
            do
            {
                code = GenerateJoinCode();
            } while (await dbContext.Hubs.AnyAsync(h => h.JoinCode == code));

            return code;
        }

        public async Task<(bool Success, string Error, Hub? Hub)> CreateHubAsync(CreateHubRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                    return (false, "Hub name cannot be empty.", null);

                if (request.Name.Length > 50)
                    return (false, "Hub name cannot exceed 50 characters.", null);

                if (!await dbContext.Users.AnyAsync(u => u.Id == request.CreatorId))
                    return (false, "Creator user does not exist.", null);

                var joinCode = await GenerateUniqueJoinCodeAsync();

                var hub = new Hub
                {
                    Name = request.Name,
                    JoinCode = joinCode,
                    CreatorId = request.CreatorId,
                    CreatedAt = DateTime.UtcNow,
                    IsPrivate = request.IsPrivate
                };

                dbContext.Hubs.Add(hub);
                await dbContext.SaveChangesAsync(); // Save to get hub.Id

                // Add creator as admin member
                var creatorMember = new HubMember
                {
                    HubId = hub.Id,
                    UserId = request.CreatorId,
                    Role = HubMemberRole.Admin,
                    JoinedAt = DateTime.UtcNow
                };

                dbContext.HubMembers.Add(creatorMember);
                await dbContext.SaveChangesAsync();

                // Reload hub without navigation properties to avoid circular reference
                hub = await dbContext.Hubs
                    .FirstAsync(h => h.Id == hub.Id);
                
                // Clear Members navigation to prevent circular reference
                hub.Members = new List<HubMember>();

                return (true, string.Empty, hub);
            }
            catch (DbUpdateException ex)
            {
                return (false, $"Database error: {ex.Message}", null);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Error, HubMember? Member)> JoinHubAsync(JoinHubRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.JoinCode))
                    return (false, "Join code cannot be empty.", null);

                var hub = await dbContext.Hubs
                    .FirstOrDefaultAsync(h => h.JoinCode == request.JoinCode);

                if (hub == null)
                    return (false, "Hub not found with the provided join code.", null);

                if (!await dbContext.Users.AnyAsync(u => u.Id == request.UserId))
                    return (false, "User does not exist.", null);

                // Check if user is already a member
                var existingMember = await dbContext.HubMembers
                    .FirstOrDefaultAsync(hm => hm.HubId == hub.Id && hm.UserId == request.UserId);

                if (existingMember != null)
                    return (false, "User is already a member of this hub.", null);

                var member = new HubMember
                {
                    HubId = hub.Id,
                    UserId = request.UserId,
                    Role = HubMemberRole.Member,
                    JoinedAt = DateTime.UtcNow
                };

                dbContext.HubMembers.Add(member);
                await dbContext.SaveChangesAsync();

                // Reload member with Hub info (not User - we already have userId)
                member = await dbContext.HubMembers
                    .Include(hm => hm.Hub)
                    .FirstAsync(hm => hm.Id == member.Id);

                // Keep HubId for join response (user might need it)
                // HubId is already set from database

                // Clear Hub.Members to prevent circular reference - set to null so it's not serialized
                if (member.Hub != null)
                {
                    member.Hub.Members = null;
                }

                // User is already ignored via JsonIgnore attribute

                return (true, string.Empty, member);
            }
            catch (DbUpdateException ex)
            {
                return (false, $"Database error: {ex.Message}", null);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Error, List<Hub>? Hubs)> GetHubsAsync(bool publicOnly = true)
        {
            try
            {
                var query = dbContext.Hubs.AsQueryable();

                if (publicOnly)
                {
                    query = query.Where(h => !h.IsPrivate);
                }

                var hubs = await query.ToListAsync();

                // Clear Members - set to null so it's not serialized
                foreach (var hub in hubs)
                {
                    hub.Members = null;
                }

                return (true, string.Empty, hubs);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Error, Hub? Hub)> GetHubByIdAsync(int hubId)
        {
            try
            {
                var hub = await dbContext.Hubs
                    .FirstOrDefaultAsync(h => h.Id == hubId);

                if (hub == null)
                    return (false, "Hub not found.", null);

                // Load members - we'll create simplified member objects
                var members = await dbContext.HubMembers
                    .Where(hm => hm.HubId == hubId)
                    .ToListAsync();

                // Create simplified member objects with only id, userId, role, joinedAt
                // hubId is required by the model but will serialize - it's redundant in this context
                // Hub and User are already ignored via JsonIgnore attributes
                var memberList = members.Select(m => new HubMember
                {
                    Id = m.Id,
                    HubId = m.HubId, // Required by model, but redundant since we're already in hub context
                    UserId = m.UserId,
                    Role = m.Role,
                    JoinedAt = m.JoinedAt,
                    Hub = null,
                    User = null
                }).ToList();
                
                hub.Members = memberList;

                return (true, string.Empty, hub);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Error)> DeleteHubAsync(int hubId, int userId)
        {
            try
            {
                var hub = await dbContext.Hubs
                    .FirstOrDefaultAsync(h => h.Id == hubId);

                if (hub == null)
                    return (false, "Hub not found.");

                // Check if user is admin of this hub
                var member = await dbContext.HubMembers
                    .FirstOrDefaultAsync(hm => hm.HubId == hubId && hm.UserId == userId);

                if (member == null || member.Role != HubMemberRole.Admin)
                    return (false, "Only hub admins can delete hubs.");

                // Delete all members first (cascade should handle this, but being explicit)
                var allMembers = await dbContext.HubMembers
                    .Where(hm => hm.HubId == hubId)
                    .ToListAsync();

                dbContext.HubMembers.RemoveRange(allMembers);

                // Delete the hub
                dbContext.Hubs.Remove(hub);
                await dbContext.SaveChangesAsync();

                return (true, string.Empty);
            }
            catch (DbUpdateException ex)
            {
                return (false, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Error)> LeaveHubAsync(int hubId, int userId)
        {
            try
            {
                var member = await dbContext.HubMembers
                    .FirstOrDefaultAsync(hm => hm.HubId == hubId && hm.UserId == userId);

                if (member == null)
                    return (false, "You are not a member of this hub.");

                // Check if user is the only admin
                var adminCount = await dbContext.HubMembers
                    .CountAsync(hm => hm.HubId == hubId && hm.Role == HubMemberRole.Admin);

                if (member.Role == HubMemberRole.Admin && adminCount == 1)
                    return (false, "Cannot leave hub as you are the only admin. Transfer admin role or delete the hub instead.");

                dbContext.HubMembers.Remove(member);
                await dbContext.SaveChangesAsync();

                return (true, string.Empty);
            }
            catch (DbUpdateException ex)
            {
                return (false, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}");
            }
        }
    }
}

