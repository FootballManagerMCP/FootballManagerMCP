using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Serialization;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>
/// An entry of a game menu. Top entries carry their fields directly; entries below come wrapped in RowData, beside
/// their own ChildList. The payload the entry sends (ActionData) stays in the game and isn't read here.
/// </summary>
internal sealed class BridgeMenuItemDto
{
    public string? Text { get; set; }
    public long? ID { get; set; }

    /// <summary>The native event picking the entry sends: EventFmxDoAction for one that does something, 0 for a sub-menu.</summary>
    public int? Event { get; set; }

    public bool? Disabled { get; set; }
    public BridgeMenuItemDto? RowData { get; set; }

    [JsonConverter(typeof(CappedListJsonConverter<BridgeMenuItemDto>))]
    public List<BridgeMenuItemDto>? ChildList { get; set; }
}
