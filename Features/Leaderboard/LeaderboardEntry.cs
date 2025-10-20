namespace PhotoScavengerHunt.Features.Leaderboard
{
    public struct LeaderboardEntry
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
    }
}


