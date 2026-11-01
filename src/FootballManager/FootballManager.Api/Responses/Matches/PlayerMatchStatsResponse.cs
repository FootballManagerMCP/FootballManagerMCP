namespace FootballManager.Api.Responses.Matches;

public class PlayerMatchStatsResponse
{
    public int? Goals { get; set; }
    public int? Assists { get; set; }
    public double? ExpectedGoals { get; set; }
    public double? ExpectedAssists { get; set; }
    public int? Shots { get; set; }
    public int? ShotsOnTarget { get; set; }
    public int? KeyPasses { get; set; }
    public int? ClearCutChances { get; set; }
    public int? ChancesCreated { get; set; }
    public int? Passes { get; set; }
    public int? PassCompletionPercentage { get; set; }
    public int? ProgressivePasses { get; set; }
    public int? Dribbles { get; set; }
    public int? Crosses { get; set; }
    public int? CrossesCompleted { get; set; }
    public int? Tackles { get; set; }
    public int? TacklesWon { get; set; }
    public int? Interceptions { get; set; }
    public int? PossessionWon { get; set; }
    public int? PossessionLost { get; set; }
    public int? AerialDuels { get; set; }
    public int? AerialDuelsWon { get; set; }
    public int? Saves { get; set; }

    /// <summary>Kilometres.</summary>
    public double? DistanceRun { get; set; }
}
