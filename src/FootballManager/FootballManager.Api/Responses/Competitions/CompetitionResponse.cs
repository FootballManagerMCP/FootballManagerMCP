namespace FootballManager.Api.Responses.Competitions;

public class CompetitionResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Nation { get; set; }

    /// <summary>The game's reputation for it: the higher, the stronger.</summary>
    public int? Reputation { get; set; }

    /// <summary>Only sent when true.</summary>
    public bool? IsUserLeague { get; set; }
}
