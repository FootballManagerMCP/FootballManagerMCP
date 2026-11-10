namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>A player's numbers in a match, as the game's player match stats record holds them.</summary>
internal sealed class BridgePlayerMatchStatsDto
{
    public BridgeReferenceDto? Player { get; set; }

    /// <summary>The player's last incident in the match (MatchConstants: came on, went off, booked, injured).</summary>
    public int? IncidentType { get; set; }

    /// <summary>The minute of that incident.</summary>
    public int? MatchTime { get; set; }

    /// <summary>The lineup slot he held at the final whistle; -1 once he had left the pitch, or never came on.</summary>
    public int? PlayerTeamSelectionIndex { get; set; }

    public bool? YellowCardedInMatch { get; set; }
    public bool? RedCardedInMatch { get; set; }
    public double? TotalRating { get; set; }
    public int? Goals { get; set; }
    public int? Assists { get; set; }
    public double? ExpectedGoals { get; set; }
    public double? ExpectedAssists { get; set; }
    public int? TotalShots { get; set; }
    public int? ShotsOnTarget { get; set; }
    public int? KeyPasses { get; set; }
    public int? ClearCutChances { get; set; }
    public int? ChancesCreated { get; set; }
    public int? PassAttempts { get; set; }
    public int? PassCompletionPercentage { get; set; }
    public int? ProgressivePasses { get; set; }
    public int? DribblesMade { get; set; }
    public int? CrossAttempts { get; set; }
    public int? CrossesCompleted { get; set; }
    public int? TackleAttempts { get; set; }
    public int? TackleCompleted { get; set; }
    public int? Interceptions { get; set; }
    public int? PossessionWon { get; set; }
    public int? PossessionLost { get; set; }
    public int? AerialDuelsAttempted { get; set; }
    public int? AerialDuelsCompleted { get; set; }
    public int? TotalSaves { get; set; }

    /// <summary>Kilometres.</summary>
    public double? DistanceRan { get; set; }
}
