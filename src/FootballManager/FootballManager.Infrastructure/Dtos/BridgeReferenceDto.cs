using System.Text.Json.Serialization;

namespace FootballManager.Infrastructure.Dtos;

/// <summary>
/// A reference to a game record as the plugin writes it, e.g. {"$ref": "Person", "index": 30647}. The index is the
/// record's place in the game's database, the same for every reference to it.
/// </summary>
internal sealed class BridgeReferenceDto
{
    [JsonPropertyName("$ref")]
    public string? Type { get; set; }

    public int? Index { get; set; }
}
