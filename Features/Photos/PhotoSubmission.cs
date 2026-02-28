using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoScavengerHunt.Features.Photos
{
    public record PhotoSubmission
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public int? ChallengeId { get; set; }  // Optional - null = global, set = challenge-specific
        public string PhotoUrl { get; set; } = string.Empty;
        public int Votes { get; set; }
        public List<Comment> Comments { get; set; } = new();

        [NotMapped]
        public string? UserName { get; set; }
    }
}