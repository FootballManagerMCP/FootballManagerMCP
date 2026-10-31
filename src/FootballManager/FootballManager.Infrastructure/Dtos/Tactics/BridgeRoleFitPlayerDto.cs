using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Tactics;

/// <summary>A squad player with their role table at one position; the game leaves it out if they can't play there.</summary>
internal sealed class BridgeRoleFitPlayerDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(TacticConstants.RoleTable)]
    public List<BridgeRoleRowDto>? Roles { get; set; }
}
