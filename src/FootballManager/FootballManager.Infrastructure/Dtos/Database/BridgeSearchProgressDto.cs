using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Database;

/// <summary>The search box: the text it searches for, and whether its results are in.</summary>
internal sealed class BridgeSearchProgressDto
{
    [JsonPropertyName(DatabaseSearchConstants.Text)]
    public string? Text { get; set; }

    [JsonPropertyName(DatabaseSearchConstants.IsFinished)]
    public bool? IsFinished { get; set; }
}
