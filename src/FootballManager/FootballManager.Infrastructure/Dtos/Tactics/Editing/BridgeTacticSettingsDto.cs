using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Tactics.Editing;

/// <summary>The active tactic's style and mentality as ids, with their options, and which slots hold a tactic.</summary>
internal sealed class BridgeTacticSettingsDto
{
    /// <summary>The active slot, 0 to 2.</summary>
    [JsonPropertyName(TacticConstants.CurrentSlot)]
    public int? CurrentSlot { get; set; }

    [JsonPropertyName(TacticConstants.Slot1Valid)]
    public bool? Slot1Valid { get; set; }

    [JsonPropertyName(TacticConstants.Slot2Valid)]
    public bool? Slot2Valid { get; set; }

    [JsonPropertyName(TacticConstants.Slot3Valid)]
    public bool? Slot3Valid { get; set; }

    [JsonPropertyName(TacticConstants.Style)]
    public BridgeOptionDto? Style { get; set; }

    [JsonPropertyName(TacticConstants.StyleOptions)]
    public List<BridgeOptionDto>? StyleOptions { get; set; }

    [JsonPropertyName(TacticConstants.MentalityValue)]
    public long? Mentality { get; set; }

    [JsonPropertyName(TacticConstants.MentalityOptions)]
    public List<BridgeOptionDto>? MentalityOptions { get; set; }
}
