using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Training;

/// <summary>The sessions one category holds, read with the category as the calendar's context.</summary>
internal sealed class BridgeSessionCategoryDto
{
    [JsonPropertyName(TrainingConstants.CategorySessions)]
    public List<BridgeOptionDto>? Sessions { get; set; }
}
