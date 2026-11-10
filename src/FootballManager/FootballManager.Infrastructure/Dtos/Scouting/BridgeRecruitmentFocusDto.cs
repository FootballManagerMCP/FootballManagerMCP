using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Tactics;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>A recruitment focus's requested properties, keyed by the game's property paths.</summary>
internal sealed class BridgeRecruitmentFocusDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(ScoutingConstants.FocusStatus)]
    public string? Status { get; set; }

    [JsonPropertyName(ScoutingConstants.FocusComplete)]
    public bool? IsComplete { get; set; }

    [JsonPropertyName(ScoutingConstants.FocusStruggling)]
    public bool? IsStruggling { get; set; }

    [JsonPropertyName(ScoutingConstants.FocusAdvice)]
    public string? Advice { get; set; }

    [JsonPropertyName(ScoutingConstants.FocusAreas)]
    public string? Areas { get; set; }

    [JsonPropertyName(ScoutingConstants.FocusScouts)]
    public string? Scouts { get; set; }

    [JsonPropertyName(ScoutingConstants.FocusCreatedBy)]
    public string? CreatedBy { get; set; }

    [JsonPropertyName(ScoutingConstants.FocusCreated)]
    public string? Created { get; set; }

    [JsonPropertyName(ScoutingConstants.FocusMinAge)]
    public BridgeOptionDto? MinAge { get; set; }

    [JsonPropertyName(ScoutingConstants.FocusMaxAge)]
    public BridgeOptionDto? MaxAge { get; set; }

    /// <summary>A star range on the game's 0-20 scale.</summary>
    [JsonPropertyName(ScoutingConstants.FocusMinAbility)]
    public BridgeRatingDto? MinAbility { get; set; }

    [JsonPropertyName(ScoutingConstants.FocusMinPotential)]
    public BridgeRatingDto? MinPotential { get; set; }

    /// <summary>Every position the focus could look for, each ticked or not.</summary>
    [JsonPropertyName(ScoutingConstants.FocusPositions)]
    public List<BridgeFocusPositionDto> Positions { get; set; } = [];

    [JsonPropertyName(ScoutingConstants.RecommendedCount)]
    public int? RecommendedCount { get; set; }

    [JsonPropertyName(ScoutingConstants.CurrentCount)]
    public int? CurrentCount { get; set; }

    [JsonPropertyName(ScoutingConstants.NearMissCount)]
    public int? NearMissCount { get; set; }
}
