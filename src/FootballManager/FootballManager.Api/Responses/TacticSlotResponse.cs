namespace FootballManager.Api.Responses;

public class TacticSlotResponse
{
    public int Index { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public TacticalEffectivenessResponse? Effectiveness { get; set; }
    public List<SlotPositionResponse> Lineup { get; set; } = [];
}
