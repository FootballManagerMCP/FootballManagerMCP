namespace FootballManager.Api.Responses.Tactics.Effectiveness;

public class PlayerAbilityScoresResponse
{
    public string? Overall { get; set; }
    public string? RoleSuitability { get; set; }
    public string? Capability { get; set; }
    public EffectivenessAdviceResponse? Advice { get; set; }
}
