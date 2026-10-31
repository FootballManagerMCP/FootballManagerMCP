using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Tactics.Effectiveness;

/// <summary>The parts of a tactic slot's tactical effectiveness, as the game's records hold them.</summary>
internal sealed class BridgeSlotEffectivenessDto
{
    [JsonPropertyName(TacticConstants.SlotOverall)]
    public BridgeRatingDto? Overall { get; set; }

    [JsonPropertyName(TacticConstants.SlotPlayerAbility)]
    public BridgePlayerAbilityDto? PlayerAbility { get; set; }

    [JsonPropertyName(TacticConstants.SlotPlayerBuyIn)]
    public BridgePlayerBuyInDto? PlayerBuyIn { get; set; }

    [JsonPropertyName(TacticConstants.SlotUnderstanding)]
    public BridgeTacticalUnderstandingDto? TacticalUnderstanding { get; set; }

    [JsonPropertyName(TacticConstants.SlotNextMatch)]
    public BridgeNextMatchDto? NextMatch { get; set; }

    /// <summary>The game's warning flags by name, e.g. "PlayerBelowThresholdWarning".</summary>
    [JsonPropertyName(TacticConstants.SlotStatusFlags)]
    public Dictionary<string, bool> StatusFlags { get; set; } = [];
}
