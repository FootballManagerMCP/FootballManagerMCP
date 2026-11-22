namespace FootballManager.Api.Responses.Matches;

public class MatchDetailResponse
{
    public required MatchSummaryResponse Summary { get; set; }
    public TeamMatchStatsResponse? TeamStats { get; set; }
    public TeamMatchStatsResponse? OpponentStats { get; set; }
    public List<MatchGoalResponse> Goals { get; set; } = [];

    /// <summary>Only sent when someone was sent off.</summary>
    public List<MatchRedCardResponse>? RedCards { get; set; }

    public PenaltyShootoutResponse? PenaltyShootout { get; set; }
    public List<MatchLineupPlayerResponse> Lineup { get; set; } = [];
    public List<MatchSubstitutionResponse> Substitutions { get; set; } = [];

    /// <summary>Only sent when true.</summary>
    public bool? LineupRearranged { get; set; }

    public PlayerOfTheMatchResponse? PlayerOfTheMatch { get; set; }

    /// <summary>Only sent while the game keeps it (the last five matches).</summary>
    public List<string>? Analysis { get; set; }
}
