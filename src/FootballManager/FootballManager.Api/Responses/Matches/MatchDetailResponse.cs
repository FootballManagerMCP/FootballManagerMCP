namespace FootballManager.Api.Responses.Matches;

public class MatchDetailResponse
{
    public required MatchSummaryResponse Summary { get; set; }
    public TeamMatchStatsResponse? TeamStats { get; set; }
    public TeamMatchStatsResponse? OpponentStats { get; set; }
    public List<MatchGoalResponse> Goals { get; set; } = [];
    public List<MatchLineupPlayerResponse> Lineup { get; set; } = [];
    public List<MatchSubstitutionResponse> Substitutions { get; set; } = [];
    public PlayerOfTheMatchResponse? PlayerOfTheMatch { get; set; }
}
