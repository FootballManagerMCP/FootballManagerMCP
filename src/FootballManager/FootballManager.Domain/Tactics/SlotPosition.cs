namespace FootballManager.Domain.Tactics;

/// <summary>One position of a saved tactic, with the player the game puts there and how effective they'd be.</summary>
public class SlotPosition
{
    public int Index { get; set; }

    /// <summary>The game's own label: its position and role abbreviations, e.g. "MOD (AI)".</summary>
    public string? InPossession { get; set; }

    public int? PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public Rating? Effectiveness { get; set; }
}
