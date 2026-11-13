namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>One of the user's shortlists as the game lists them: its name, its size as text, and its place.</summary>
internal sealed class BridgeShortlistDto
{
    /// <summary>The list's name; the default list's is the game's label for it.</summary>
    public string? DisplayString { get; set; }

    /// <summary>Its size as the game writes it, e.g. "14 players".</summary>
    public string? CountString { get; set; }

    public int? Index { get; set; }
    public bool? IsDefault { get; set; }
}
