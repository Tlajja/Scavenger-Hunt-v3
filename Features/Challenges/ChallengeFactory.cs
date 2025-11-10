namespace PhotoScavengerHunt.Features.Challenges
{
    public static class ChallengeFactory
    {
        public static Challenge Create(string name, int taskId, int creatorId, bool isPrivate = false, string joinCode = "", DateTime? deadline = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Challenge name cannot be empty.", nameof(name));

            return new Challenge
            {
                Name = name,
                TaskId = taskId,
                CreatorId = creatorId,
                CreatedAt = DateTime.UtcNow,
                Deadline = deadline ?? DateTime.UtcNow.AddDays(7),
                IsPrivate = isPrivate,
                JoinCode = joinCode,
                Status = ChallengeStatus.Open

            };
        }
    }
}
