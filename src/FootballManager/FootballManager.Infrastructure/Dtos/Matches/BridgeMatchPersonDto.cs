using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>A player of one side's match lines, as the match itself names him.</summary>
internal sealed class BridgeMatchPersonDto
{
    [JsonPropertyName(MatchConstants.PlayerStatsPlayer)]
    public BridgeReferenceDto? Player { get; set; }

    [JsonPropertyName(MatchConstants.PlayerStatsName)]
    public string? Name { get; set; }

    [JsonPropertyName(MatchConstants.PlayerStatsId)]
    public int? Id { get; set; }
}
