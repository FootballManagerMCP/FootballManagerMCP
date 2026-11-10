using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>The sizes of the user's scouting lists, read from the user's manager.</summary>
internal sealed class BridgeScoutingCountsDto
{
    [JsonPropertyName(ScoutingConstants.CentreCounts)]
    public BridgeCentreCountsDto? Centre { get; set; }

    [JsonPropertyName(ScoutingConstants.ShortlistCount)]
    public int? ShortlistedPlayers { get; set; }

    [JsonPropertyName(ScoutingConstants.ScoutedPlayersCount)]
    public int? ScoutedPlayers { get; set; }

    /// <summary>Which focuses the scouting screen lists (ScoutingConstants.FocusListType).</summary>
    [JsonPropertyName(ScoutingConstants.FocusListTypeFromHuman)]
    public int? FocusListType { get; set; }
}
