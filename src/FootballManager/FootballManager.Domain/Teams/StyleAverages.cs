namespace FootballManager.Domain.Teams;

/// <summary>A side's numbers per match, averaged: how a team plays (or what its opponents manage against it).</summary>
public class StyleAverages
{
    public double? Possession { get; set; }
    public double? ExpectedGoals { get; set; }
    public double? Shots { get; set; }
    public double? ShotsOnTarget { get; set; }
    public double? LongShots { get; set; }
    public double? ChancesCreated { get; set; }
    public double? Crosses { get; set; }
    public double? CrossCompletionPercentage { get; set; }
    public double? Passes { get; set; }
    public double? PassCompletionPercentage { get; set; }
    public double? ProgressivePasses { get; set; }
    public double? FinalThirdPasses { get; set; }

    /// <summary>Opposition passes allowed per defensive action (PPDA): the lower, the harder the press.</summary>
    public double? PassesPerDefensiveAction { get; set; }

    public double? Tackles { get; set; }
    public double? HeaderCompletionPercentage { get; set; }
    public double? Fouls { get; set; }
}
