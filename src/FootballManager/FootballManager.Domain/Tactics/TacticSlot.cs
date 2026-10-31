using FootballManager.Domain.Tactics.Effectiveness;

namespace FootballManager.Domain.Tactics;

/// <summary>One of the three tactics the user can keep ready in the game.</summary>
public class TacticSlot
{
    /// <summary>0, 1 or 2, in the game's order.</summary>
    public int Index { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public TacticalEffectiveness? Effectiveness { get; set; }
    public List<SlotPosition> Lineup { get; set; } = [];
}
