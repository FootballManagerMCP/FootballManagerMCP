namespace FootballManager.Domain.Tactics.Editing;

/// <summary>One of the eleven slots: its player, and the position and role key of each phase.</summary>
public class EditableSlot
{
    /// <summary>The game's index, 0 to 10.</summary>
    public int Index { get; set; }

    public int? PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public ByPhase<TacticalPosition> Position { get; set; } = new();
    public ByPhase<string> Role { get; set; } = new();
}
