using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Teams;

/// <summary>The user's league, from the user's manager.</summary>
internal sealed class BridgeLeagueDto
{
    [JsonPropertyName(TeamConstants.LeagueName)]
    public string? Name { get; set; }
}
