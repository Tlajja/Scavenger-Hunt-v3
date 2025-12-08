using System.Collections.Generic;
using PhotoScavengerHunt.Features.Photos;

namespace PhotoScavengerHunt.Services.Interfaces;

public interface ICommentService
{
    Task<(bool Success, string Error, List<object>? Comments)> AddCommentAsync(int submissionId, AddCommentRequest request);
    Task<(bool Success, string Error, List<object>? Comments)> GetCommentsAsync(int submissionId);
    Task<(bool Success, string Error)> DeleteCommentAsync(int submissionId, int commentId);
}

