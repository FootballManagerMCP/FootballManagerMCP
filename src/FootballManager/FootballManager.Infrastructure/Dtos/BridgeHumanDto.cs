using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos;

/// <summary>Properties of the user's manager (the plugin's "Human" root).</summary>
internal sealed class BridgeHumanDto
{
    [JsonPropertyName(GameConstants.TeamName)]
    public string? TeamName { get; set; }
}
