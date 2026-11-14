using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>The user's shortlists, and the one the shortlist screen shows.</summary>
internal sealed class BridgeShortlistsDto
{
    [JsonPropertyName(ScoutingConstants.Shortlists)]
    public List<BridgeShortlistDto>? Shortlists { get; set; }

    [JsonPropertyName(ScoutingConstants.SelectedShortlist)]
    public BridgeShortlistDto? Selected { get; set; }
}
