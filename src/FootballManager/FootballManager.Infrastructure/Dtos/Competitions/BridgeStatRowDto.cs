using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Serialization;

namespace FootballManager.Infrastructure.Dtos.Competitions;

/// <summary>A row of a stat's table: the player, his team, the stat's value and his rank.</summary>
internal sealed class BridgeStatRowDto
{
    [JsonPropertyName(CompetitionConstants.RowPlayerId)]
    public int? PlayerId { get; set; }

    [JsonPropertyName(CompetitionConstants.RowName)]
    public string? Name { get; set; }

    [JsonPropertyName(CompetitionConstants.RowAge)]
    public int? Age { get; set; }

    /// <summary>Every position he can play, as a mask of position bits (no side bits).</summary>
    [JsonPropertyName(CompetitionConstants.RowPositions)]
    public ulong? Positions { get; set; }

    [JsonPropertyName(CompetitionConstants.RowMinutes), JsonConverter(typeof(StatValueJsonConverter))]
    public double? Minutes { get; set; }

    [JsonPropertyName(CompetitionConstants.RowTeamId)]
    public int? TeamId { get; set; }

    [JsonPropertyName(CompetitionConstants.RowTeam)]
    public string? Team { get; set; }

    [JsonPropertyName(CompetitionConstants.RowValue)]
    public double? Value { get; set; }

    /// <summary>As the game writes it: "1st", "2nd", or "=" when tied with the row above.</summary>
    [JsonPropertyName(CompetitionConstants.RowRank)]
    public string? Rank { get; set; }
}
