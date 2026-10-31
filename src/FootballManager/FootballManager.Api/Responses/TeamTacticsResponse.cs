namespace FootballManager.Api.Responses;

public class TeamTacticsResponse
{
    public required string TeamName { get; set; }
    public List<TacticSlotResponse> Slots { get; set; } = [];
    public required ActiveTacticResponse Active { get; set; }
    public List<string> RecommendedStyles { get; set; } = [];
}
