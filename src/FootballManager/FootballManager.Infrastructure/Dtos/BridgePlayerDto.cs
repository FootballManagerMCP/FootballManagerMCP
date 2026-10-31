using System.Text.Json;
using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos;

/// <summary>A player's requested properties, keyed by the game's property paths.</summary>
internal sealed class BridgePlayerDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(GameConstants.Age)]
    public int? Age { get; set; }

    [JsonPropertyName(GameConstants.Positions)]
    public string? Positions { get; set; }

    [JsonPropertyName(GameConstants.BestRole)]
    public string? BestRole { get; set; }

    [JsonPropertyName(GameConstants.Personality)]
    public string? Personality { get; set; }

    [JsonPropertyName(GameConstants.Morale)]
    public string? Morale { get; set; }

    /// <summary>Percent; the game sends -1 while the player is injured.</summary>
    [JsonPropertyName(GameConstants.Condition)]
    public int? Condition { get; set; }

    [JsonPropertyName(GameConstants.Sharpness)]
    public int? MatchSharpness { get; set; }

    [JsonPropertyName(GameConstants.IsInjured)]
    public bool? IsInjured { get; set; }

    [JsonPropertyName(GameConstants.AverageRating)]
    public double? AverageRating { get; set; }

    [JsonPropertyName(GameConstants.CurrentAbility)]
    public int? CurrentAbility { get; set; }

    [JsonPropertyName(GameConstants.PotentialAbility)]
    public int? PotentialAbility { get; set; }

    /// <summary>
    /// Every other requested path: the attribute range bounds, keyed like "AttributeFinishing.AttributeMinValue".
    /// The game sends 0 for a bound it masks (attributes are 1-20).
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> AttributeBounds { get; set; } = [];
}
