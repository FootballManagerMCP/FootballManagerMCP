using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Tactics.Editing;

/// <summary>The matchday squad's places, as the lineup's player dropdown lists them.</summary>
internal sealed class BridgeSelectionDto
{
    [JsonPropertyName(TacticConstants.SelectionDropdown)]
    public List<BridgeSelectionRowDto>? Rows { get; set; }
}
