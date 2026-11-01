namespace FootballManager.Api.Responses.Matches;

public class MatchGoalResponse
{
    public required string Minute { get; set; }
    public bool IsOurs { get; set; }
    public string? Scorer { get; set; }
    public string? Assister { get; set; }
}
