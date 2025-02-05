namespace ConsoleApp1.Models
{
    internal class PlayerScore
    {
        public Guid Id { get; set; }
        public string GuidShort { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public string Status { get; set; }
    }

    internal class RequestModel
    {
        public PlayerScore PlayerScore { get; set; } = new PlayerScore();
        public string RequestType { get; set; }
    }
}
