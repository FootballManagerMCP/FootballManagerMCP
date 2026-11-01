namespace FootballManager.Api.Responses.Players;

public class PlayerSeasonStatsResponse
{
    public int? Appearances { get; set; }
    public int? Minutes { get; set; }
    public int? Goals { get; set; }
    public int? Assists { get; set; }
    public double? ExpectedGoals { get; set; }
    public double? NonPenaltyExpectedGoals { get; set; }
    public double? ExpectedGoalsOverperformance { get; set; }
    public double? ExpectedGoalsPerShot { get; set; }
    public double? ExpectedAssists { get; set; }
    public double? ShotsPer90 { get; set; }
    public int? ShotsOnTarget { get; set; }
    public int? KeyPasses { get; set; }
    public int? ChancesCreated { get; set; }
    public int? PassCompletionPercentage { get; set; }
}
