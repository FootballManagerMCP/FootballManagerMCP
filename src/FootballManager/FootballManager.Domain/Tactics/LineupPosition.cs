namespace FootballManager.Domain.Tactics;

/// <summary>One of the eleven slots of the active tactic, with the player picked for it.</summary>
public class LineupPosition
{
    /// <summary>The slot's index, 0 (goalkeeper) to 10, as the game numbers them.</summary>
    public int Index { get; set; }
    public int? PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public PositionAssignment InPossession { get; set; } = new();
    public PositionAssignment OutOfPossession { get; set; } = new();
}
