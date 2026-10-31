using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos;

/// <summary>A role's key attributes: each one's display name, with the given player's value in it.</summary>
internal sealed class BridgeKeyAttributesDto
{
    [JsonPropertyName(TacticConstants.KeyAttributesForRole)]
    public List<BridgeOptionDto>? Attributes { get; set; }
}
