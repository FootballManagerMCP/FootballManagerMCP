using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Tactics;

/// <summary>The role table of the player in a lineup slot, at that slot's position.</summary>
internal sealed class BridgeLineupRoleTableDto
{
    [JsonPropertyName(TacticConstants.LineupRoleTable)]
    public List<BridgeRoleRowDto>? Roles { get; set; }
}
