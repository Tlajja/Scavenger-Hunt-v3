using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Services.Interfaces;
using PhotoScavengerHunt.Repositories;

namespace PhotoScavengerHunt.Services
{
    public class VotesService : IVotesService
    {
        private readonly IPhotoRepository _photoRepo;

        public VotesService(IPhotoRepository photoRepo)
        {
            _photoRepo = photoRepo;
        }

        public async Task<(bool Success, string? ErrorMessage, PhotoSubmission? Result)> UpvotePhotoAsync(int submissionId, int userId)
        {
            var submission = await _photoRepo.FindByIdAsync(submissionId);
            if (submission == null)
                return (false, "Submission not found.", null);
            
            if (submission.UserId == userId)
                return (false, "You cannot vote for your own submission.", null);

            // Check if user already voted for this submission
            var existingVote = await _photoRepo.GetUserVoteForSubmissionAsync(submissionId, userId);
            if (existingVote != null)
                return (false, "You have already voted for this submission.", null);

            var result = await _photoRepo.UpvoteAsync(submissionId, userId);
            if (!result.Success) 
                return result;
            
            await _photoRepo.SaveChangesAsync();
            return (true, null, result.Result);
        }

        public async Task<(bool Success, string? ErrorMessage, PhotoSubmission? Result)> RemoveVoteAsync(int submissionId, int userId)
        {
            var submission = await _photoRepo.FindByIdAsync(submissionId);
            if (submission == null)
                return (false, "Submission not found.", null);

            var existingVote = await _photoRepo.GetUserVoteForSubmissionAsync(submissionId, userId);
            if (existingVote == null)
                return (false, "You have not voted for this submission.", null);

            var result = await _photoRepo.RemoveVoteAsync(submissionId, userId);
            if (!result.Success)
                return result;

            await _photoRepo.SaveChangesAsync();
            return (true, null, result.Result);
        }

        public async Task<Dictionary<int, bool>> GetUserVotesForTaskAsync(int taskId, int userId)
        {
            return await _photoRepo.GetUserVotesForTaskAsync(taskId, userId);
        }

        public async Task<Dictionary<int, bool>> GetUserVotesForChallengeAsync(int challengeId, int userId)
        {
            return await _photoRepo.GetUserVotesForChallengeAsync(challengeId, userId);
        }
    }
}