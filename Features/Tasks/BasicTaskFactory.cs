namespace PhotoScavengerHunt.Features.Tasks
{
    public static class BasicTaskFactory
    {

        public static BasicTask Create(string description, int authorId, DateTime? deadline = null)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Task description cannot be empty.", nameof(description));

            return new BasicTask
            {
                Description = description,
                AuthorId = authorId,
                Deadline = deadline
            };
        }
    }
}