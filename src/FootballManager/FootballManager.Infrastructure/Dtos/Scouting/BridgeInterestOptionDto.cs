using System.Text.Json.Serialization;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>An interest type, with the level asked for when it's the one selected.</summary>
internal sealed class BridgeInterestOptionDto
{
    public int? Id { get; set; }

    [JsonPropertyName("PlayerInterestLevelForType")]
    public int? Level { get; set; }
}
