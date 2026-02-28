using System;

namespace PhotoScavengerHunt.Exceptions
{
    public class LimitExceededException : Exception
    {
        public LimitExceededException(string message) : base(message) { }
    }
}
