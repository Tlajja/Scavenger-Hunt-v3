namespace PhotoScavengerHunt.Features.Photos
{
    public record PhotoSubmission
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public int? HubId { get; set; }  // Optional - null = global, set = hub-specific
        public string PhotoUrl { get; set; } = string.Empty;
        public int Votes { get; set; }
        public List<Comment> Comments { get; set; } = new();
    }
}