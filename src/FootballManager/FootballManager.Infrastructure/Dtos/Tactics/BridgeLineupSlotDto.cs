using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Tactics;

/// <summary>One lineup slot of the active tactic in one phase (in or out of possession).</summary>
internal sealed class BridgeLineupSlotDto
{
    [JsonPropertyName(TacticConstants.LineupPlayerId)]
    public int? PlayerId { get; set; }

    [JsonPropertyName(TacticConstants.LineupPlayerName)]
    public string? PlayerName { get; set; }

    /// <summary>A position mask (see PositionMasks).</summary>
    [JsonPropertyName(TacticConstants.PositionValue)]
    public ulong? Position { get; set; }

    /// <summary>A role mask: one bit per role, numbered separately in and out of possession.</summary>
    [JsonPropertyName(TacticConstants.RoleValue)]
    public ulong? Role { get; set; }

    [JsonPropertyName(TacticConstants.LineupRoleRating)]
    public BridgeRatingDto? Rating { get; set; }
}
