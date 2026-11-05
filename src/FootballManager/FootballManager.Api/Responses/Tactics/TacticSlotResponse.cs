using FootballManager.Api.Responses.Tactics.Effectiveness;

namespace FootballManager.Api.Responses.Tactics;

public class TacticSlotResponse
{
    /// <summary>1, 2 or 3, as the game numbers them (and set_tactic takes them).</summary>
    public int Slot { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public TacticalEffectivenessResponse? Effectiveness { get; set; }
    public List<SlotPositionResponse> Lineup { get; set; } = [];
}
