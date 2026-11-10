using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Tactics;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>The focus wizard's draft, as the game holds it while it's filled in.</summary>
internal sealed class BridgeDraftFocusDto
{
    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(ScoutingConstants.IsValid)]
    public bool? IsValid { get; set; }

    /// <summary>The name of the focus the draft's name clashes with; empty when none.</summary>
    [JsonPropertyName(ScoutingConstants.DuplicateName)]
    public string? DuplicateName { get; set; }

    [JsonPropertyName(ScoutingConstants.AreaCount)]
    public int? AreaCount { get; set; }

    [JsonPropertyName(ScoutingConstants.FocusPositions)]
    public List<BridgeFocusPositionDto> Positions { get; set; } = [];

    [JsonPropertyName(ScoutingConstants.FocusMinAbility)]
    public BridgeRatingDto? MinAbility { get; set; }

    [JsonPropertyName(ScoutingConstants.FocusMinPotential)]
    public BridgeRatingDto? MinPotential { get; set; }

    [JsonPropertyName(ScoutingConstants.MinAgeChoices)]
    public List<BridgeOptionDto> MinAgeChoices { get; set; } = [];

    [JsonPropertyName(ScoutingConstants.MaxAgeChoices)]
    public List<BridgeOptionDto> MaxAgeChoices { get; set; } = [];
}
