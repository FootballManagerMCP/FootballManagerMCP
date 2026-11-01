namespace FootballManager.Domain.Players;

/// <summary>A player's numbers this season, all competitive matches for the club together.</summary>
public class PlayerSeasonStats
{
    public int? Appearances { get; set; }
    public int? Minutes { get; set; }
    public int? Goals { get; set; }
    public int? Assists { get; set; }
    public double? ExpectedGoals { get; set; }
    public double? NonPenaltyExpectedGoals { get; set; }

    /// <summary>Goals minus expected goals: below zero means scoring less than the chances were worth.</summary>
    public double? ExpectedGoalsOverperformance { get; set; }

    /// <summary>The average quality of the player's shots.</summary>
    public double? ExpectedGoalsPerShot { get; set; }
    public double? ExpectedAssists { get; set; }
    public double? ShotsPer90 { get; set; }
    public int? ShotsOnTarget { get; set; }
    public int? KeyPasses { get; set; }
    public int? ChancesCreated { get; set; }
    public int? PassCompletionPercentage { get; set; }
}
