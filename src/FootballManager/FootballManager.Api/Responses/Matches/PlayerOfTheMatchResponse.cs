namespace FootballManager.Api.Responses.Matches;

public class PlayerOfTheMatchResponse
{
    public required string Name { get; set; }
    public double? Rating { get; set; }
    public bool IsOurs { get; set; }
}
