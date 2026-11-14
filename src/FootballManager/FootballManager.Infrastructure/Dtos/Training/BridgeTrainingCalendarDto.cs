using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Training;

/// <summary>The user's training calendar read for today: this week and the next, and the session categories.</summary>
internal sealed class BridgeTrainingCalendarDto
{
    [JsonPropertyName(TrainingConstants.Weeks)]
    public List<BridgeTrainingWeekDto>? Weeks { get; set; }

    [JsonPropertyName(TrainingConstants.Categories)]
    public List<BridgeOptionDto>? Categories { get; set; }

    [JsonPropertyName(TrainingConstants.Schedule)]
    public string? Schedule { get; set; }
}
