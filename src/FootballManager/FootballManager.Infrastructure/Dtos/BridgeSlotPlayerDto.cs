using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos;

/// <summary>A player in a tactic slot's lineup, with how effective the game rates them there.</summary>
internal sealed class BridgeSlotPlayerDto
{
    [JsonPropertyName(TacticConstants.SlotPlayerId)]
    public int? Id { get; set; }

    [JsonPropertyName(TacticConstants.SlotPlayerName)]
    public string? Name { get; set; }

    [JsonPropertyName(TacticConstants.SlotPlayerEffectiveness)]
    public BridgeRatingDto? Effectiveness { get; set; }
}
