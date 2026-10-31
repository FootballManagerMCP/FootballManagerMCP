namespace FootballManager.Infrastructure.Dtos.Tactics.Effectiveness;

internal sealed class BridgeNextMatchDto
{
    public BridgeRatingDto? OverallEffectiveness { get; set; }
    public BridgeAdviceDto? AdviceForComponent { get; set; }
}
