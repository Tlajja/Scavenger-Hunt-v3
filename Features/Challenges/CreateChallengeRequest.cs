namespace PhotoScavengerHunt.Features.Challenges;

public record CreateChallengeRequest(
    string Name,
    int TaskId,
    int CreatorId,
    DateTime Deadline,
    bool IsPrivate = false
);
