namespace FootballManager.Api.Responses.Matches;

/// <summary>A match from the user's side: "for" is the user's team, "against" the opponent.</summary>
public class MatchSummaryResponse
{
    public DateOnly Date { get; set; }
    public string? Competition { get; set; }

    /// <summary>Only present (true) for friendlies.</summary>
    public bool? IsFriendly { get; set; }
    public required string Venue { get; set; }
    public required string Opponent { get; set; }

    /// <summary>The opponent's current league position as the game writes it, for league matches.</summary>
    public string? OpponentLeaguePosition { get; set; }
    public string? Outcome { get; set; }
    public int? GoalsFor { get; set; }
    public int? GoalsAgainst { get; set; }
    public double? ExpectedGoalsFor { get; set; }
    public double? ExpectedGoalsAgainst { get; set; }

    /// <summary>The user's share of the ball, in percent.</summary>
    public int? Possession { get; set; }
    public int? ShotsFor { get; set; }
    public int? ShotsAgainst { get; set; }
    public int? ShotsOnTargetFor { get; set; }
    public int? ShotsOnTargetAgainst { get; set; }

    /// <summary>The shape the user's team lined up in with the ball, e.g. "4-2-2-2".</summary>
    public string? Formation { get; set; }
}
