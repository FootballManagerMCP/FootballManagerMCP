namespace FootballManager.Domain.Scouting;

/// <summary>The game's player search as its screen shows it, in the game's display language.</summary>
public class PlayerSearchStatus
{
    /// <summary>The results' heading, e.g. "players found (Filter active)" or "(Search in progress...)".</summary>
    public string? Status { get; set; }

    /// <summary>How interested a player must be in joining, e.g. "Can be persuaded".</summary>
    public string? Interest { get; set; }

    /// <summary>The attribute filter, e.g. "No attribute selected".</summary>
    public string? Attributes { get; set; }
}
