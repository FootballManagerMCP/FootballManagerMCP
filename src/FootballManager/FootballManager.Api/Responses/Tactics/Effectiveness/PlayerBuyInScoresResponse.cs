namespace FootballManager.Api.Responses.Tactics.Effectiveness;

public class PlayerBuyInScoresResponse
{
    public string? Overall { get; set; }
    public string? ManagerSupport { get; set; }
    public string? ManagerTendencies { get; set; }
    public string? Relationships { get; set; }
    public EffectivenessAdviceResponse? Advice { get; set; }
}
