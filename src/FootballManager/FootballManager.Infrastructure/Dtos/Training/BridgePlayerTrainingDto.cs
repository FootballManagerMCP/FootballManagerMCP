using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Training;

/// <summary>A training squad player and his own training, as the individual training screen reads it.</summary>
internal sealed class BridgePlayerTrainingDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    /// <summary>The position he trains in, as a tactics position mask.</summary>
    [JsonPropertyName(TrainingConstants.Position)]
    public ulong? Position { get; set; }

    /// <summary>The position and role picked for training: 0 for what he plays in matches.</summary>
    [JsonPropertyName(TrainingConstants.PositionChoice)]
    public long? PositionChoice { get; set; }

    /// <summary>The in-possession role he trains for, as a role mask.</summary>
    [JsonPropertyName(TrainingConstants.RoleId)]
    public ulong? RoleId { get; set; }

    [JsonPropertyName(TrainingConstants.RoleName)]
    public string? RoleName { get; set; }

    [JsonPropertyName(TrainingConstants.OutOfPossessionRole)]
    public BridgeOptionDto? OutOfPossessionRole { get; set; }

    [JsonPropertyName(TrainingConstants.Focus)]
    public BridgeOptionDto? Focus { get; set; }

    [JsonPropertyName(TrainingConstants.FocusCanEdit)]
    public bool? FocusCanEdit { get; set; }

    [JsonPropertyName(TrainingConstants.FocusHumanControlled)]
    public bool? FocusHumanControlled { get; set; }

    [JsonPropertyName(TrainingConstants.Intensity)]
    public BridgeOptionDto? Intensity { get; set; }

    [JsonPropertyName(TrainingConstants.IntensityAutomatic)]
    public bool? IntensityAutomatic { get; set; }

    [JsonPropertyName(TrainingConstants.IntensityCanEdit)]
    public bool? IntensityCanEdit { get; set; }

    [JsonPropertyName(TrainingConstants.RestDays)]
    public int? RestDays { get; set; }

    [JsonPropertyName(TrainingConstants.Rating)]
    public double? Rating { get; set; }

    [JsonPropertyName(TrainingConstants.Workload)]
    public string? Workload { get; set; }

    /// <summary>His focus dropdown: rows, some after a separator.</summary>
    [JsonPropertyName(TrainingConstants.FocusList)]
    public List<BridgeOptionDto>? FocusChoices { get; set; }

    /// <summary>His intensity dropdown: rows, the rest periods under "no work".</summary>
    [JsonPropertyName(TrainingConstants.IntensityList)]
    public List<BridgeOptionDto>? IntensityChoices { get; set; }
}
