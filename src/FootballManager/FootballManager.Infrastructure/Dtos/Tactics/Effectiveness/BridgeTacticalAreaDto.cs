namespace FootballManager.Infrastructure.Dtos.Tactics.Effectiveness;

internal sealed class BridgeTacticalAreaDto
{
    public BridgeRatingDto? OverallEffectiveness { get; set; }
    public BridgeRatingDto? MatchExposure { get; set; }
    public BridgeRatingDto? TrainingContribution { get; set; }
}
