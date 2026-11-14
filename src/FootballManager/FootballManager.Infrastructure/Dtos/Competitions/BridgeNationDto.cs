using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Competitions;

/// <summary>A nation the game plays out, as listed on the user's continent.</summary>
internal sealed class BridgeNationDto
{
    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }
}
