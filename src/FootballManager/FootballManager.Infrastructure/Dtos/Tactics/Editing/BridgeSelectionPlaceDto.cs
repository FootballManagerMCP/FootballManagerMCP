using System.Text.Json.Serialization;

namespace FootballManager.Infrastructure.Dtos.Tactics.Editing;

/// <summary>A place's team selection index and its player: a reference with his database index, or a null reference.</summary>
internal sealed class BridgeSelectionPlaceDto
{
    public int? Index { get; set; }

    [JsonPropertyName("player")]
    public BridgeReferenceDto? Player { get; set; }
}
