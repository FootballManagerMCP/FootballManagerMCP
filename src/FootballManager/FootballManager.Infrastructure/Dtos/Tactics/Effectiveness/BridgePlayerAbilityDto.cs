namespace FootballManager.Infrastructure.Dtos.Tactics.Effectiveness;

internal sealed class BridgePlayerAbilityDto
{
    public BridgeRatingDto? OverallEffectiveness { get; set; }
    public BridgeRatingDto? RoleSuitabilityScore { get; set; }
    public BridgeRatingDto? CapabilityScore { get; set; }
    public BridgeAdviceDto? AdviceForComponent { get; set; }
}
