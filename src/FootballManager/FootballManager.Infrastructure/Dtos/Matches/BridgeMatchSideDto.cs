using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>One side's numbers in a match: the team's, and its players' when asked for.</summary>
internal sealed class BridgeMatchSideDto
{
    [JsonPropertyName(MatchConstants.TeamStats)]
    public BridgeTeamMatchStatsDto? Stats { get; set; }

    [JsonPropertyName(MatchConstants.PlayerStats)]
    public List<BridgePlayerMatchStatsDto>? Players { get; set; }
}
