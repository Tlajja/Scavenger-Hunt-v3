namespace PhotoScavengerHunt.Features.Challenges
{
    public static class ChallengeFactory
    {
        public static Challenge Create(string name, int creatorId, IEnumerable<int> taskIds, bool isPrivate = false, string joinCode = "", DateTime? deadline = null, int? maxParticipants = null, TimeSpan? submissionDuration = null, TimeSpan? votingDuration = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Challenge name cannot be empty.", nameof(name));

            var challenge = new Challenge
            {
                Name = name,
                CreatorId = creatorId,
                CreatedAt = DateTime.UtcNow,
                Deadline = deadline ?? DateTime.UtcNow.AddDays(7),
                IsPrivate = isPrivate,
                JoinCode = joinCode,
                Status = ChallengeStatus.Open,
                MaxParticipants = maxParticipants ?? 10,
                ChallengeTasks = new List<ChallengeTask>()
            };

            var subDur = submissionDuration ?? TimeSpan.FromDays(1);
            var voteDur = votingDuration ?? TimeSpan.FromDays(1);
            challenge.SubmissionEndsAt = challenge.CreatedAt.Add(subDur);
            challenge.VotingEndsAt = challenge.SubmissionEndsAt?.Add(voteDur);

            foreach (var id in taskIds.Distinct())
            {
                challenge.ChallengeTasks.Add(new ChallengeTask { Challenge = challenge, TaskId = id, Deadline = null });
            }

            return challenge;
        }
    }
}