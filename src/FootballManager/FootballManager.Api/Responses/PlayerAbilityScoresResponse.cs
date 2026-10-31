namespace FootballManager.Api.Responses;

public class PlayerAbilityScoresResponse
{
    public string? Overall { get; set; }
    public string? RoleSuitability { get; set; }
    public string? Capability { get; set; }
    public EffectivenessAdviceResponse? Advice { get; set; }
}
