namespace FootballManager.Api.Responses.Matches;

public class MatchRedCardResponse
{
    public required string Minute { get; set; }
    public bool IsOurs { get; set; }
    public string? Player { get; set; }
}
