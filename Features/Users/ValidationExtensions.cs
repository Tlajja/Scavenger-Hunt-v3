using System.Text.RegularExpressions;

namespace PhotoScavengerHunt.Features.Users
{
    public static class ValidationExtensions
    {
        private static readonly Regex UsernameRegex = new("^[a-zA-Z0-9]+$");
        
        public static bool IsValidUsername(string username)
        {
            if(string.IsNullOrWhiteSpace(username))
            {
                return false;
            }
            if(username.Length < 2 || username.Length > 20)
            {
                return false;
            }

            return UsernameRegex.IsMatch(username);
        }
    }
}