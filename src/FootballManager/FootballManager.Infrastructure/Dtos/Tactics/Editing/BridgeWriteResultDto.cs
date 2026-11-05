using System.Text.Json;

namespace FootballManager.Infrastructure.Dtos.Tactics.Editing;

/// <summary>The plugin's answer to a /set: the property's value before and after, as the game reads it back.</summary>
internal sealed class BridgeWriteResultDto
{
    public string? Path { get; set; }

    /// <summary>A number, or for a dropdown the chosen option ({PropertyValue, DisplayValue}).</summary>
    public JsonElement? Before { get; set; }

    public JsonElement? After { get; set; }
    public bool? Changed { get; set; }
}
