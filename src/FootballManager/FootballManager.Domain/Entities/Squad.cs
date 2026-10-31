namespace FootballManager.Domain.Entities;

/// <summary>The user's first-team squad, read live from the running game.</summary>
public class Squad
{
    public required string TeamName { get; set; }
    public List<Player> Players { get; set; } = [];

    /// <summary>True when hidden data was asked for but the game masked all of it (no In-Game Editor).</summary>
    public bool HiddenDataMasked { get; set; }
}
