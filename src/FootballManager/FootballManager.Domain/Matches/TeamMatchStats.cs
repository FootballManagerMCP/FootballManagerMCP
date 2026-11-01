namespace FootballManager.Domain.Matches;

/// <summary>One team's numbers in a match.</summary>
public class TeamMatchStats
{
    public double? ExpectedGoals { get; set; }
    public double? NonPenaltyExpectedGoals { get; set; }
    public int? Possession { get; set; }
    public int? Shots { get; set; }
    public int? ShotsOnTarget { get; set; }
    public int? LongShots { get; set; }
    public int? ShotsHitWoodwork { get; set; }
    public int? ChancesCreated { get; set; }
    public int? Corners { get; set; }
    public int? Crosses { get; set; }
    public int? CrossCompletionPercentage { get; set; }
    public int? Passes { get; set; }
    public int? PassCompletionPercentage { get; set; }
    public int? ProgressivePasses { get; set; }
    public int? FinalThirdPasses { get; set; }

    /// <summary>Opposition passes allowed per defensive action (PPDA): the lower, the more intense the press.</summary>
    public double? PassesPerDefensiveAction { get; set; }
    public int? Tackles { get; set; }
    public int? TackleCompletionPercentage { get; set; }
    public int? Headers { get; set; }
    public int? HeaderCompletionPercentage { get; set; }
    public int? Offsides { get; set; }
    public int? Fouls { get; set; }
    public int? YellowCards { get; set; }
    public int? RedCards { get; set; }
    public double? AverageRating { get; set; }
}
