namespace FootballManager.Api.Responses.Competitions;

public class CompetitionStatColumnResponse
{
    public required string Stat { get; set; }

    /// <summary>In the game's display language.</summary>
    public string? Label { get; set; }
}
