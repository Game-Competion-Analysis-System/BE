namespace DAL.DTO
{
    public class PlayerDto
    {
        public int PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public string? GuildName { get; set; }
        public double LatestScore { get; set; }
        public int LatestRank { get; set; }
        public string? GameName { get; set; }
        public string? ServerName { get; set; }
    }
}
