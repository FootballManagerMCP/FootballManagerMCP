using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos;

internal sealed class BridgeFormationDto
{
    [JsonPropertyName(TacticConstants.FormationName)]
    public string? Name { get; set; }
}
