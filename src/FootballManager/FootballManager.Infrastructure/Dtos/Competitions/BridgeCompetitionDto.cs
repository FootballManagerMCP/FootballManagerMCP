using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Competitions;

/// <summary>A competition's requested properties, keyed by the game's property paths.</summary>
internal sealed class BridgeCompetitionDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(CompetitionConstants.Reputation)]
    public int? Reputation { get; set; }

    [JsonPropertyName(CompetitionConstants.NationName)]
    public string? Nation { get; set; }

    /// <summary>A league split in parts (Wales' second tier: North and South) holds them as children.</summary>
    [JsonPropertyName(CompetitionConstants.ChildCount)]
    public int? ChildCount { get; set; }
}
