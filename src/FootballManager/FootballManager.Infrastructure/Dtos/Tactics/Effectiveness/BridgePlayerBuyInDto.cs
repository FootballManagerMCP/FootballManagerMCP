namespace FootballManager.Infrastructure.Dtos.Tactics.Effectiveness;

internal sealed class BridgePlayerBuyInDto
{
    public BridgeRatingDto? OverallEffectiveness { get; set; }
    public BridgeRatingDto? ManagerSupportScore { get; set; }
    public BridgeRatingDto? ManagerTendencies { get; set; }
    public BridgeRatingDto? TeammateAndStaffRelationshipsScore { get; set; }
    public BridgeAdviceDto? AdviceForComponent { get; set; }
}
