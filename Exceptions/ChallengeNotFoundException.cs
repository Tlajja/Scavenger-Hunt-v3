namespace PhotoScavengerHunt.Exceptions
{
    public class ChallengeNotFoundException : Exception
    {
        public ChallengeNotFoundException() { }

        public ChallengeNotFoundException(string message) : base(message) { }

        public ChallengeNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}

