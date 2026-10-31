namespace FootballManager.Infrastructure.Dtos;

internal sealed class BridgeTacticalUnderstandingDto
{
    public BridgeRatingDto? OverallEffectiveness { get; set; }
    public BridgeRatingDto? TacticalComprehension { get; set; }
    public BridgeAdviceDto? AdviceForTacticalAreas { get; set; }
    public BridgeAdviceDto? AdviceForTacticalComprehension { get; set; }
    public BridgeTacticalAreaDto? InPossessionShape { get; set; }
    public BridgeTacticalAreaDto? OutOfPossessionShape { get; set; }
    public BridgeTacticalAreaDto? BuildUpStrategy { get; set; }
    public BridgeTacticalAreaDto? ChanceCreation { get; set; }
    public BridgeTacticalAreaDto? PassingStyle { get; set; }
    public BridgeTacticalAreaDto? PressingIntensity { get; set; }
    public BridgeTacticalAreaDto? PressingTrap { get; set; }
    public BridgeTacticalAreaDto? LineOfEngagement { get; set; }
    public BridgeTacticalComplexityDto? TacticalComplexity { get; set; }
}
