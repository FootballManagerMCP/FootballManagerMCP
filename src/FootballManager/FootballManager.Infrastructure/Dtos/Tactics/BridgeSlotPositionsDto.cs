using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Tactics;

/// <summary>
/// A tactic slot's positions as the game lists them for a player: their own slot first, then all eleven by index
/// (PropertyValue), labelled with position and role abbreviations.
/// </summary>
internal sealed class BridgeSlotPositionsDto
{
    [JsonPropertyName(TacticConstants.SlotPositions)]
    public List<BridgeOptionDto>? Positions { get; set; }
}
