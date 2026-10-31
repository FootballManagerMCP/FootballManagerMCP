namespace FootballManager.Domain.Tactics;

/// <summary>The user's tactics, read live from the running game: the three slots, and the active one in full.</summary>
public class TeamTactics
{
    public required string TeamName { get; set; }
    public List<TacticSlot> Slots { get; set; } = [];
    public required ActiveTactic Active { get; set; }

    /// <summary>The tactical styles the game recommends for this squad, best first.</summary>
    public List<string> RecommendedStyles { get; set; } = [];
}
