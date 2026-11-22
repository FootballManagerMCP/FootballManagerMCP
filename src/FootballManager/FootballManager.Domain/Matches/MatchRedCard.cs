namespace FootballManager.Domain.Matches;

/// <summary>A player sent off, from either side.</summary>
public class MatchRedCard
{
    /// <summary>As the game writes it, e.g. "90+1'".</summary>
    public required string Minute { get; set; }

    public bool IsOurs { get; set; }
    public string? Player { get; set; }
}
