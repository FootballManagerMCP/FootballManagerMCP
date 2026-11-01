namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>A team's numbers in a match, as the game's match stats record holds them.</summary>
internal sealed class BridgeTeamMatchStatsDto
{
    public double? ExpectedGoals { get; set; }
    public double? NonPenaltyExpectedGoals { get; set; }
    public int? TotalPossession { get; set; }
    public int? TotalShots { get; set; }
    public int? ShotsOnTarget { get; set; }
    public int? LongShots { get; set; }
    public int? ShotHitWoodwork { get; set; }
    public int? ChancesCreated { get; set; }
    public int? Corners { get; set; }
    public int? CrossAttempts { get; set; }
    public int? CrossCompletionPercentage { get; set; }
    public int? PassAttempts { get; set; }
    public int? PassCompletionPercentage { get; set; }
    public int? ProgressivePasses { get; set; }
    public int? FinalThirdPasses { get; set; }
    public double? OppositionPassesPerDefensiveAction { get; set; }
    public int? TackleAttempts { get; set; }
    public int? TackleCompletionPercentage { get; set; }
    public int? HeaderAttempts { get; set; }
    public int? HeaderCompletionPercentage { get; set; }
    public int? Offsides { get; set; }
    public int? FoulsMade { get; set; }
    public int? YellowCards { get; set; }
    public int? RedCards { get; set; }
    public double? AverageRatingFloat { get; set; }
}
