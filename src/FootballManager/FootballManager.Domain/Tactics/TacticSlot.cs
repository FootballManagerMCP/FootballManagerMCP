using FootballManager.Domain.Tactics.Effectiveness;

namespace FootballManager.Domain.Tactics;

/// <summary>One of the three tactics the user can keep ready in the game.</summary>
public class TacticSlot
{
    /// <summary>1, 2 or 3, as the game numbers them.</summary>
    public int Slot { get; set; }

    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public TacticalEffectiveness? Effectiveness { get; set; }
    public List<SlotPosition> Lineup { get; set; } = [];
}
