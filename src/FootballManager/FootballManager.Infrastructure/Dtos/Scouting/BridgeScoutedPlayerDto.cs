using System.Text.Json;
using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Players;
using FootballManager.Infrastructure.Dtos.Tactics;
using FootballManager.Infrastructure.Serialization;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>A player outside the club as the user's scouting knows him, keyed by the game's property paths.</summary>
internal sealed class BridgeScoutedPlayerDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(GameConstants.Age)]
    public int? Age { get; set; }

    [JsonPropertyName(GameConstants.Positions)]
    public string? Positions { get; set; }

    [JsonPropertyName(ScoutingConstants.ClubName)]
    public string? Club { get; set; }

    [JsonPropertyName(ScoutingConstants.NationName)]
    public string? Nation { get; set; }

    [JsonPropertyName(ScoutingConstants.Recommendation)]
    public string? Recommendation { get; set; }

    [JsonPropertyName(ScoutingConstants.GradeLabel)]
    public string? GradeLabel { get; set; }

    [JsonPropertyName(ScoutingConstants.Grade)]
    public int? Grade { get; set; }

    [JsonPropertyName(ScoutingConstants.AbilityStars)]
    public BridgeRatingDto? AbilityStars { get; set; }

    [JsonPropertyName(ScoutingConstants.PotentialStars)]
    public BridgeRatingDto? PotentialStars { get; set; }

    /// <summary>Formatted by the game, e.g. "7,76M €".</summary>
    [JsonPropertyName(ScoutingConstants.MinValue)]
    public string? MinValue { get; set; }

    [JsonPropertyName(ScoutingConstants.MaxValue)]
    public string? MaxValue { get; set; }

    [JsonPropertyName(ScoutingConstants.MinWage)]
    public BridgeSortedValueDto? MinWage { get; set; }

    [JsonPropertyName(ScoutingConstants.MaxWage)]
    public BridgeSortedValueDto? MaxWage { get; set; }

    /// <summary>A position mask, as the tactics screen's.</summary>
    [JsonPropertyName(ScoutingConstants.BestPosition)]
    public ulong? BestPosition { get; set; }

    /// <summary>A role mask, as the tactics screen's in-possession roles.</summary>
    [JsonPropertyName(ScoutingConstants.BestRole)]
    public ulong? BestRole { get; set; }

    [JsonPropertyName(ScoutingConstants.BestOutOfPossessionRole)]
    public ulong? BestOutOfPossessionRole { get; set; }

    [JsonPropertyName(ScoutingConstants.StrongestArea)]
    public string? StrongestArea { get; set; }

    [JsonPropertyName(ScoutingConstants.WeakestArea)]
    public string? WeakestArea { get; set; }

    [JsonPropertyName(ScoutingConstants.Knowledge)]
    public BridgeKnowledgeDto? Knowledge { get; set; }

    [JsonPropertyName(ScoutingConstants.IsShortlisted)]
    public bool? IsShortlisted { get; set; }

    [JsonPropertyName(ScoutingConstants.IsBeingScouted)]
    public bool? IsBeingScouted { get; set; }

    [JsonPropertyName(ScoutingConstants.DaysSinceLastScouted)]
    public BridgeSortedValueDto? DaysSinceLastScouted { get; set; }

    [JsonPropertyName(GameConstants.Statuses)]
    [JsonConverter(typeof(CappedListJsonConverter<BridgePlayerStatusDto>))]
    public List<BridgePlayerStatusDto>? Statuses { get; set; }

    [JsonPropertyName(GameConstants.TeamStatuses)]
    [JsonConverter(typeof(CappedListJsonConverter<BridgePlayerStatusDto>))]
    public List<BridgePlayerStatusDto>? TeamStatuses { get; set; }

    [JsonPropertyName(GameConstants.CurrentAbility)]
    public int? CurrentAbility { get; set; }

    [JsonPropertyName(GameConstants.PotentialAbility)]
    public int? PotentialAbility { get; set; }

    // The latest report, only present when asked for.
    [JsonPropertyName(ScoutingConstants.ReportDate)]
    public string? ReportDate { get; set; }

    [JsonPropertyName(ScoutingConstants.ReportScout)]
    public string? ReportScout { get; set; }

    [JsonPropertyName(ScoutingConstants.ReportAbility)]
    public string? ReportAbility { get; set; }

    [JsonPropertyName(ScoutingConstants.ReportPotential)]
    public string? ReportPotential { get; set; }

    [JsonPropertyName(ScoutingConstants.ReportProsCons)]
    public BridgeProsConsDto? ReportProsCons { get; set; }

    [JsonPropertyName(ScoutingConstants.ReportTransfer)]
    public BridgeTransferInfoDto? ReportTransfer { get; set; }

    /// <summary>
    /// Every other requested path: the attribute range bounds, keyed like "AttributeFinishing.AttributeMinValue".
    /// The game sends 0 for a bound it masks (attributes are 1-20).
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> AttributeBounds { get; set; } = [];
}
