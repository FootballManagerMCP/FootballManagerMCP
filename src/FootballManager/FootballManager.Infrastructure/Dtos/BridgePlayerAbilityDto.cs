namespace FootballManager.Infrastructure.Dtos;

internal sealed class BridgePlayerAbilityDto
{
    public BridgeRatingDto? OverallEffectiveness { get; set; }
    public BridgeRatingDto? RoleSuitabilityScore { get; set; }
    public BridgeRatingDto? CapabilityScore { get; set; }
    public BridgeAdviceDto? AdviceForComponent { get; set; }
}
