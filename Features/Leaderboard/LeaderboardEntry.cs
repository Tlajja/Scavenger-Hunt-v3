namespace PhotoScavengerHunt.Features.Leaderboard
{
    public struct LeaderboardEntry : IComparable<LeaderboardEntry>
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int TotalVotes { get; set; }

        public LeaderboardEntry(int userId, string userName, int totalVotes)
        {
            UserId = userId;
            UserName = userName;
            TotalVotes = totalVotes;
        }

        // Defines the natural ordering — higher scores first
        public int CompareTo(LeaderboardEntry other)
        {
            // Sort descending by TotalVotes
            int voteComparison = other.TotalVotes.CompareTo(TotalVotes);
            if (voteComparison != 0)
                return voteComparison;

            // Tie-break by name (alphabetical)
            int nameComparison = string.Compare(UserName, other.UserName, StringComparison.OrdinalIgnoreCase);
            if (nameComparison != 0)
                return nameComparison;

            // Final tie-break by UserId (ascending)
            return UserId.CompareTo(other.UserId);
        }
    }
}