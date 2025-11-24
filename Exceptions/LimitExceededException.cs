using System;

namespace PhotoScavengerHunt.Exceptions
{
    public class LimitExceededException : Exception
    {
        public LimitExceededException() { }

        public LimitExceededException(string message) : base(message) { }

        public LimitExceededException(string message, Exception inner) : base(message, inner) { }
    }
}
