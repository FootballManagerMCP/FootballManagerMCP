using System.Text.Json;
using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Tactics;

/// <summary>The active tactic's settings and the three slots' names, keyed by the game's property paths.</summary>
internal sealed class BridgeTacticOverviewDto
{
    /// <summary>The active slot, 0 to 2.</summary>
    [JsonPropertyName(TacticConstants.CurrentSlot)]
    public int? CurrentSlot { get; set; }

    [JsonPropertyName(TacticConstants.Slot1Name)]
    public string? Slot1Name { get; set; }

    [JsonPropertyName(TacticConstants.Slot2Name)]
    public string? Slot2Name { get; set; }

    [JsonPropertyName(TacticConstants.Slot3Name)]
    public string? Slot3Name { get; set; }

    [JsonPropertyName(TacticConstants.Slot1Valid)]
    public bool? Slot1Valid { get; set; }

    [JsonPropertyName(TacticConstants.Slot2Valid)]
    public bool? Slot2Valid { get; set; }

    [JsonPropertyName(TacticConstants.Slot3Valid)]
    public bool? Slot3Valid { get; set; }

    [JsonPropertyName(TacticConstants.Mentality)]
    public string? Mentality { get; set; }

    [JsonPropertyName(TacticConstants.Style)]
    public BridgeOptionDto? Style { get; set; }

    [JsonPropertyName(TacticConstants.StyleOptions)]
    public List<BridgeOptionDto>? StyleOptions { get; set; }

    /// <summary>Style ids, as the options list them.</summary>
    [JsonPropertyName(TacticConstants.RecommendedStyles)]
    public List<long>? RecommendedStyles { get; set; }

    /// <summary>Every other requested path: the team instructions, each holding the id of its chosen option.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> TeamInstructions { get; set; } = [];
}
