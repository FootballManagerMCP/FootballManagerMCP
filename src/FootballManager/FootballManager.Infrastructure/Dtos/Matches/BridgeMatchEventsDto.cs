using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>A match's incidents (goals and the like) and its player of the match.</summary>
internal sealed class BridgeMatchEventsDto
{
    [JsonPropertyName(MatchConstants.Incidents)]
    public List<BridgeIncidentDto>? Incidents { get; set; }

    [JsonPropertyName(MatchConstants.PlayerOfTheMatchId)]
    public int? PlayerOfTheMatchId { get; set; }

    [JsonPropertyName(MatchConstants.PlayerOfTheMatchName)]
    public string? PlayerOfTheMatchName { get; set; }

    [JsonPropertyName(MatchConstants.PlayerOfTheMatchRating)]
    public double? PlayerOfTheMatchRating { get; set; }
}
