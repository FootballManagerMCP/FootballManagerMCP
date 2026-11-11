using System.Text.Json;
using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Serialization;

namespace FootballManager.Infrastructure.Dtos.Players;

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

    [JsonPropertyName(GameConstants.Statuses)]
    [JsonConverter(typeof(CappedListJsonConverter<BridgePlayerStatusDto>))]
    public List<BridgePlayerStatusDto>? Statuses { get; set; }

    [JsonPropertyName(GameConstants.TeamStatuses)]
    [JsonConverter(typeof(CappedListJsonConverter<BridgePlayerStatusDto>))]
    public List<BridgePlayerStatusDto>? TeamStatuses { get; set; }

    [JsonPropertyName(GameConstants.TrialIsValid)]
    public bool? TrialIsValid { get; set; }

    // yyyy-MM-dd, as the plugin writes the game's dates.
    [JsonPropertyName(GameConstants.TrialEndDate)]
    public string? TrialEndDate { get; set; }

    [JsonPropertyName(GameConstants.CurrentAbility)]
    public int? CurrentAbility { get; set; }

    [JsonPropertyName(GameConstants.PotentialAbility)]
    public int? PotentialAbility { get; set; }

    // This season's numbers, only present when asked for.
    [JsonPropertyName(GameConstants.Appearances), JsonConverter(typeof(StatValueJsonConverter))]
    public double? Appearances { get; set; }

    [JsonPropertyName(GameConstants.Minutes), JsonConverter(typeof(StatValueJsonConverter))]
    public double? Minutes { get; set; }

    [JsonPropertyName(GameConstants.Goals), JsonConverter(typeof(StatValueJsonConverter))]
    public double? Goals { get; set; }

    [JsonPropertyName(GameConstants.Assists), JsonConverter(typeof(StatValueJsonConverter))]
    public double? Assists { get; set; }

    [JsonPropertyName(GameConstants.ExpectedGoals), JsonConverter(typeof(StatValueJsonConverter))]
    public double? ExpectedGoals { get; set; }

    [JsonPropertyName(GameConstants.NonPenaltyExpectedGoals), JsonConverter(typeof(StatValueJsonConverter))]
    public double? NonPenaltyExpectedGoals { get; set; }

    [JsonPropertyName(GameConstants.ExpectedGoalsOverperformance), JsonConverter(typeof(StatValueJsonConverter))]
    public double? ExpectedGoalsOverperformance { get; set; }

    [JsonPropertyName(GameConstants.ExpectedGoalsPerShot), JsonConverter(typeof(StatValueJsonConverter))]
    public double? ExpectedGoalsPerShot { get; set; }

    [JsonPropertyName(GameConstants.ExpectedAssists), JsonConverter(typeof(StatValueJsonConverter))]
    public double? ExpectedAssists { get; set; }

    [JsonPropertyName(GameConstants.ShotsPer90), JsonConverter(typeof(StatValueJsonConverter))]
    public double? ShotsPer90 { get; set; }

    [JsonPropertyName(GameConstants.ShotsOnTarget), JsonConverter(typeof(StatValueJsonConverter))]
    public double? ShotsOnTarget { get; set; }

    [JsonPropertyName(GameConstants.KeyPasses), JsonConverter(typeof(StatValueJsonConverter))]
    public double? KeyPasses { get; set; }

    [JsonPropertyName(GameConstants.ChancesCreated), JsonConverter(typeof(StatValueJsonConverter))]
    public double? ChancesCreated { get; set; }

    [JsonPropertyName(GameConstants.PassCompletionPercentage), JsonConverter(typeof(StatValueJsonConverter))]
    public double? PassCompletionPercentage { get; set; }

    /// <summary>
    /// Every other requested path: the attribute range bounds, keyed like "AttributeFinishing.AttributeMinValue".
    /// The game sends 0 for a bound it masks (attributes are 1-20).
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> AttributeBounds { get; set; } = [];
}
