namespace FootballManager.Domain.Competitions;

/// <summary>A league the game plays out in full this season, so its stats screen ranks its players.</summary>
public class CompetitionInfo
{
    /// <summary>FM's unique id for the competition.</summary>
    public int Id { get; set; }

    /// <summary>As the game names it, in its display language.</summary>
    public required string Name { get; set; }

    public string? Nation { get; set; }

    /// <summary>The game's reputation for the competition: the higher, the stronger (the user's league sets the scale).</summary>
    public int? Reputation { get; set; }

    public bool IsUserLeague { get; set; }
}
