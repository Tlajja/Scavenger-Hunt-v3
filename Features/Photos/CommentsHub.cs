using Microsoft.AspNetCore.SignalR;

namespace PhotoScavengerHunt.Features.Photos;

public class CommentsHub : Hub
{
    public static string GetSubmissionGroupName(int submissionId) => $"submission-{submissionId}";

    public Task JoinSubmission(int submissionId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, GetSubmissionGroupName(submissionId));
    }

    public Task LeaveSubmission(int submissionId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GetSubmissionGroupName(submissionId));
    }
}


