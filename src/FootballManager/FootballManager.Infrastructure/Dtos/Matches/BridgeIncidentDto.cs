using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>Something that happened in a match (MatchConstants: goals, red cards, shoot-out penalties), with its players.</summary>
internal sealed class BridgeIncidentDto
{
    [JsonPropertyName(MatchConstants.IncidentType)]
    public int? IncidentType { get; set; }

    /// <summary>The side it happened to: 0 the home team, 1 the away team. An own goal's is the side it counts for.</summary>
    [JsonPropertyName(MatchConstants.IncidentSide)]
    public int? Side { get; set; }

    /// <summary>The minute as the game writes it, e.g. "21'" or "90+2'".</summary>
    [JsonPropertyName(MatchConstants.IncidentMinute)]
    public string? Minute { get; set; }

    /// <summary>The scorer, the player sent off, or the penalty taker.</summary>
    [JsonPropertyName(MatchConstants.IncidentPlayer)]
    public BridgeReferenceDto? Player { get; set; }

    [JsonPropertyName(MatchConstants.IncidentPlayerName)]
    public string? PlayerName { get; set; }

    [JsonPropertyName(MatchConstants.IncidentPlayerId)]
    public int? PlayerId { get; set; }

    /// <summary>A goal's assister; the game sends an empty reference when there's none.</summary>
    [JsonPropertyName(MatchConstants.IncidentSecondPlayer)]
    public BridgeReferenceDto? SecondPlayer { get; set; }

    [JsonPropertyName(MatchConstants.IncidentSecondPlayerName)]
    public string? SecondPlayerName { get; set; }
}
