namespace FootballManager.Infrastructure.Dtos.Tactics.Effectiveness;

internal sealed class BridgeTacticalComplexityDto
{
    public BridgeRatingDto? OverallEffectiveness { get; set; }
    public BridgeRatingDto? TeamInstructionComplexity { get; set; }
    public BridgeRatingDto? PlayerInstructionComplexity { get; set; }
    public BridgeRatingDto? OppositionInstructionComplexity { get; set; }
}
