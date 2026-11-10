using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Teams;

/// <summary>A club's first team, reached through the game's search: its id, name and league.</summary>
internal sealed class BridgeSearchedTeamDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(TeamConstants.DivisionName)]
    public string? League { get; set; }
}
