using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>The player search's state as its screen shows it.</summary>
internal sealed class BridgeSearchStatusDto
{
    [JsonPropertyName(ScoutingConstants.SearchStatus)]
    public string? Status { get; set; }

    [JsonPropertyName(ScoutingConstants.SearchInterest)]
    public string? Interest { get; set; }

    [JsonPropertyName(ScoutingConstants.SearchAttributes)]
    public string? Attributes { get; set; }
}
