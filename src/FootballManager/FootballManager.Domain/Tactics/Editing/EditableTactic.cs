namespace FootballManager.Domain.Tactics.Editing;

/// <summary>The active tactic by key, as a tactic edit plans from it, with the squad its players come from.</summary>
public class EditableTactic
{
    /// <summary>The slot the team plays, 1 to 3.</summary>
    public int ActiveSlot { get; set; }

    /// <summary>The slots holding a tactic, 1 to 3.</summary>
    public List<int> Slots { get; set; } = [];

    public string? Style { get; set; }
    public string? Mentality { get; set; }
    public ByPhase<string> Formation { get; set; } = new();

    /// <summary>Team instruction key to the option it holds.</summary>
    public Dictionary<string, string?> TeamInstructions { get; set; } = [];

    /// <summary>The eleven slots, by the game's index (0 to 10).</summary>
    public List<EditableSlot> Lineup { get; set; } = [];

    /// <summary>Every first-team player, by FM id.</summary>
    public Dictionary<int, string> Squad { get; set; } = [];
}
