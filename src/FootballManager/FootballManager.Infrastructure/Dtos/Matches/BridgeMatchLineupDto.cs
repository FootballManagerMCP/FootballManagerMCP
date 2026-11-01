using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>The user's players in a past match with their position and role in the phase asked for.</summary>
internal sealed class BridgeMatchLineupDto
{
    [JsonPropertyName(MatchConstants.Lineup)]
    public List<BridgeLineupEntryDto>? Entries { get; set; }
}
