namespace PhotoScavengerHunt.Features.Tasks
{
    public static class HuntTaskFactory
    {

        public static HuntTask Create(string description, int authorId, DateTime? deadline = null, HuntTaskStatus status = HuntTaskStatus.Open)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Task description cannot be empty.", nameof(description));

            return new HuntTask
            {
                Description = description,
                Deadline = deadline ?? DateTime.UtcNow.AddDays(7),
                Status = status,
                AuthorId = authorId
            };
        }
    }
}


