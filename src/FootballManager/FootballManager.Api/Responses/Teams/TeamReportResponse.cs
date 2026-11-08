using FootballManager.Api.Responses.Matches;
using FootballManager.Api.Responses.Players;

namespace FootballManager.Api.Responses.Teams;

public class TeamReportResponse
{
    public int TeamId { get; set; }
    public required string TeamName { get; set; }

    /// <summary>Competitive matches only.</summary>
    public required MatchRecordResponse Overall { get; set; }

    public required MatchRecordResponse Home { get; set; }
    public required MatchRecordResponse Away { get; set; }

    public required StyleProfileResponse Style { get; set; }
    public required StyleProfileResponse HomeStyle { get; set; }
    public required StyleProfileResponse AwayStyle { get; set; }

    public required UsualLineupResponse Lineup { get; set; }
    public List<PlayerResponse> Squad { get; set; } = [];

    /// <summary>Seen from the team's side.</summary>
    public List<MatchSummaryResponse> Played { get; set; } = [];

    public List<MatchSummaryResponse> Upcoming { get; set; } = [];
}
