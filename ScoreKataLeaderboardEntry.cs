namespace ScoreKata
{
    [System.Serializable]
    public class ScoreKataLeaderboardEntry
    {
        public string username { get; set; }
        public string score { get; set; }
        public string platform { get; set; }
        public string score_count { get; set; }
        public int rank { get; set; }
        public string user_id { get; set; }
        
        //Create an implicit operator to LeaderboardRowData
        public static implicit operator LeaderboardRowData(ScoreKataLeaderboardEntry row)
        {
            return new LeaderboardRowData
            {
                member = row.user_id,
                score = row.score,
                member_data = new LeaderboardRowMemberData
                {
                    platform = row.platform,
                    user_name = row.username
                },
                rank = row.rank,
            };
        }
    }
}