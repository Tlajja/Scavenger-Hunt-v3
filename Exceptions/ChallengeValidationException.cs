namespace PhotoScavengerHunt.Exceptions
{
    public class ChallengeValidationException : Exception
    {
        public ChallengeValidationException() { }

        public ChallengeValidationException(string message) : base(message) { }

        public ChallengeValidationException(string message, Exception inner) : base(message, inner) { }
    }
}