using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Tactics;

internal sealed class BridgeFormationDto
{
    [JsonPropertyName(TacticConstants.FormationName)]
    public string? Name { get; set; }

    /// <summary>The formation's id (FM.UI.Formation); 0 for a custom shape.</summary>
    [JsonPropertyName(TacticConstants.Formation)]
    public long? Id { get; set; }
}
