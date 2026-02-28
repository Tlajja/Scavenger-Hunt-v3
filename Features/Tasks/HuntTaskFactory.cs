namespace PhotoScavengerHunt.Features.Tasks
{
    public static class HuntTaskFactory
    {

        public static HuntTask Create(string description, int authorId, DateTime? deadline = null, int? timerSeconds = null)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Task description cannot be empty.", nameof(description));

            return new HuntTask
            {
                Description = description,
                AuthorId = authorId,
                Deadline = deadline,
                TimerSeconds = timerSeconds
            };
        }
    }
}