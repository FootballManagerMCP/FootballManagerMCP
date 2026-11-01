using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>A fixture's summary as the game holds it, home side then away side, with the user's lineup in possession.</summary>
internal sealed class BridgeFixtureDto
{
    /// <summary>"yyyy-MM-dd".</summary>
    [JsonPropertyName(MatchConstants.Date)]
    public string? Date { get; set; }

    [JsonPropertyName(MatchConstants.Competition)]
    public string? Competition { get; set; }

    [JsonPropertyName(MatchConstants.IsFriendly)]
    public bool? IsFriendly { get; set; }

    [JsonPropertyName(MatchConstants.HasResult)]
    public bool? HasResult { get; set; }

    [JsonPropertyName(MatchConstants.HomeTeamId)]
    public int? HomeTeamId { get; set; }

    [JsonPropertyName(MatchConstants.HomeTeamName)]
    public string? HomeTeamName { get; set; }

    [JsonPropertyName(MatchConstants.AwayTeamId)]
    public int? AwayTeamId { get; set; }

    [JsonPropertyName(MatchConstants.AwayTeamName)]
    public string? AwayTeamName { get; set; }

    /// <summary>Sent as text; -1 until the match is played.</summary>
    [JsonPropertyName(MatchConstants.HomeScore), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? HomeScore { get; set; }

    [JsonPropertyName(MatchConstants.AwayScore), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? AwayScore { get; set; }

    [JsonPropertyName(MatchConstants.HomeExpectedGoals)]
    public double? HomeExpectedGoals { get; set; }

    [JsonPropertyName(MatchConstants.AwayExpectedGoals)]
    public double? AwayExpectedGoals { get; set; }

    [JsonPropertyName(MatchConstants.HomePossession)]
    public int? HomePossession { get; set; }

    [JsonPropertyName(MatchConstants.AwayPossession)]
    public int? AwayPossession { get; set; }

    [JsonPropertyName(MatchConstants.HomeShots)]
    public int? HomeShots { get; set; }

    [JsonPropertyName(MatchConstants.AwayShots)]
    public int? AwayShots { get; set; }

    [JsonPropertyName(MatchConstants.HomeShotsOnTarget)]
    public int? HomeShotsOnTarget { get; set; }

    [JsonPropertyName(MatchConstants.AwayShotsOnTarget)]
    public int? AwayShotsOnTarget { get; set; }

    [JsonPropertyName(MatchConstants.HomeLeaguePosition)]
    public string? HomeLeaguePosition { get; set; }

    [JsonPropertyName(MatchConstants.AwayLeaguePosition)]
    public string? AwayLeaguePosition { get; set; }

    /// <summary>The user's players with their in-possession position and role; absent before the match.</summary>
    [JsonPropertyName(MatchConstants.Lineup)]
    public List<BridgeLineupEntryDto>? Lineup { get; set; }
}
