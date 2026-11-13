namespace FootballManager.Domain.Scouting;

/// <summary>One of the user's shortlists ("sélections" in a French game): the game keeps a default one and any made by name.</summary>
public class Shortlist
{
    /// <summary>As the game shows it; the default list's name is the game's label for it, e.g. "(Default)".</summary>
    public required string Name { get; set; }

    public int? Players { get; set; }

    public bool IsDefault { get; set; }

    /// <summary>The list the shortlist screen shows, which get_scouted_players reads as the shortlist.</summary>
    public bool IsSelected { get; set; }
}
