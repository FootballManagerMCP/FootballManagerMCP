using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>One of the club's scouts, keyed by the game's property paths.</summary>
internal sealed class BridgeScoutDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(ScoutingConstants.Job)]
    public string? Job { get; set; }

    [JsonPropertyName(ScoutingConstants.JudgingAbility)]
    public int? JudgingAbility { get; set; }

    [JsonPropertyName(ScoutingConstants.JudgingPotential)]
    public int? JudgingPotential { get; set; }
}
