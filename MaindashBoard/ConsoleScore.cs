public class ConsoleScore
{
    public Guid Id { get; set; }
    public string GuidShort { get; set; }

    public string Name { get; set; }
    public int Score { get; set; }
    public string Status { get; set; }
}

public class RequestModel
{
    public ConsoleScore PlayerScore { get; set; }
    public string RequestType { get; set; }
}