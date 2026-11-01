namespace FootballManager.Api.Responses.Matches;

public class SeasonMatchesResponse
{
    public required string TeamName { get; set; }

    /// <summary>Competitive matches only.</summary>
    public required MatchRecordResponse Overall { get; set; }
    public required MatchRecordResponse Home { get; set; }
    public required MatchRecordResponse Away { get; set; }

    public List<MatchSummaryResponse> Played { get; set; } = [];
    public List<MatchSummaryResponse> Upcoming { get; set; } = [];
}
