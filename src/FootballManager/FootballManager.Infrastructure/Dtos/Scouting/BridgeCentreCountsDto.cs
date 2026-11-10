namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>The scouting centre's tallies, as its tabs show them.</summary>
internal sealed class BridgeCentreCountsDto
{
    public int? RecommendedPlayerCount { get; set; }
    public int? HighlyRecommendedPlayersCount { get; set; }
    public int? RequestedReportsCount { get; set; }
}
