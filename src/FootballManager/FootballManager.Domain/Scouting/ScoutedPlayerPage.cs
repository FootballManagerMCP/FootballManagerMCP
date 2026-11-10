namespace FootballManager.Domain.Scouting;

/// <summary>The players read from a scouting list, and how many it holds in all.</summary>
public class ScoutedPlayerPage
{
    public List<ScoutedPlayer> Players { get; set; } = [];

    /// <summary>The list's size; more than <see cref="Players"/> when only its first players were read.</summary>
    public int Total { get; set; }

    /// <summary>The search screen's state, for the player search.</summary>
    public PlayerSearchStatus? Search { get; set; }
}
