using System.Text.Json;
using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>The menu actions the game offers for some records, keyed "#" and the action's id.</summary>
internal sealed class BridgeActionsDto
{
    [JsonPropertyName(ScoutingConstants.Actions)]
    public Dictionary<string, JsonElement>? Actions { get; set; }
}
