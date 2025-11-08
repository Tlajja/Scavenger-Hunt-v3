using System;

namespace PhotoScavengerHunt.Exceptions
{
    public class ChallengeLimitException : Exception
    {
        public ChallengeLimitException() { }

        public ChallengeLimitException(string message) : base(message) { }

        public ChallengeLimitException(string message, Exception inner) : base(message, inner) { }
    }
}
