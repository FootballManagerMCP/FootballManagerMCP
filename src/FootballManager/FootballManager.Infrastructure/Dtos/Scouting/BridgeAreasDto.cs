using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>The scouting area dropdown, read from the user's team.</summary>
internal sealed class BridgeAreasDto
{
    [JsonPropertyName(ScoutingConstants.AreasOnTeam)]
    public List<BridgeAreaOptionDto> Options { get; set; } = [];
}
