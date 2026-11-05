using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Tactics.Editing;

/// <summary>A phase's formation id (0 for a custom shape) and the formations the game offers for it.</summary>
internal sealed class BridgeFormationChoiceDto
{
    [JsonPropertyName(TacticConstants.Formation)]
    public long? Formation { get; set; }

    /// <summary>The game's recommendations, then every formation grouped by back line, then a "formation card" entry.</summary>
    [JsonPropertyName(TacticConstants.FormationOptions)]
    public List<BridgeOptionDto>? Options { get; set; }
}
