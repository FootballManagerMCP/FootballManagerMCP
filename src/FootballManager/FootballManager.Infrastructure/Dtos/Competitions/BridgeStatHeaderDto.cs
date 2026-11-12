using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Competitions;

/// <summary>A stat table's heading, the stat's name in the game's display language.</summary>
internal sealed class BridgeStatHeaderDto
{
    [JsonPropertyName(CompetitionConstants.Label)]
    public string? Label { get; set; }
}
