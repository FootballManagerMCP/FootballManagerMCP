using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Scouting;

internal sealed class BridgeQuickFilterStateDto
{
    [JsonPropertyName(PlayerSearchConstants.QuickFilterEnabled)]
    public bool? Enabled { get; set; }
}
