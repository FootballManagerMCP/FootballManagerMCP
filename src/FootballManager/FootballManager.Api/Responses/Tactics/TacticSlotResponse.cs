using FootballManager.Api.Responses.Tactics.Effectiveness;

namespace FootballManager.Api.Responses.Tactics;

public class TacticSlotResponse
{
    public int Index { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public TacticalEffectivenessResponse? Effectiveness { get; set; }
    public List<SlotPositionResponse> Lineup { get; set; } = [];
}
