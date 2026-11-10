using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>How many active recruitment focuses the club has, whatever the scouting screen lists.</summary>
internal sealed class BridgeFocusCountDto
{
    [JsonPropertyName(ScoutingConstants.FocusCountFromHuman)]
    public int? Count { get; set; }
}
