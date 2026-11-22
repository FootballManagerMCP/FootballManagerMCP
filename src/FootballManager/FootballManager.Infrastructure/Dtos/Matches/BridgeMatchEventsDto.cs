using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>A match's player of the match and the game's post-match analysis.</summary>
internal sealed class BridgeMatchEventsDto
{
    [JsonPropertyName(MatchConstants.PlayerOfTheMatchId)]
    public int? PlayerOfTheMatchId { get; set; }

    [JsonPropertyName(MatchConstants.PlayerOfTheMatchName)]
    public string? PlayerOfTheMatchName { get; set; }

    [JsonPropertyName(MatchConstants.PlayerOfTheMatchRating)]
    public double? PlayerOfTheMatchRating { get; set; }

    /// <summary>Empty once the match is more than five matches old.</summary>
    [JsonPropertyName(MatchConstants.Analysis)]
    public List<BridgeAnalysisPointDto>? Analysis { get; set; }
}
