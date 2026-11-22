using System.Globalization;

namespace FootballManager.Infrastructure.Constants;

// The game's property paths and contexts for fixtures, as its fixtures and match screens bind them.
internal static class MatchConstants
{
    // The team's fixtures this season, oldest first. The contexts size the list like the fixtures screen does.
    public const string FixtureList = "Team.FixtureList";
    public const string PreviousAmount = "PreviousAmount";
    public const string FutureAmount = "FutureAmount";
    public const string FixturePadding = "FixturePadding";
    public const string YearIndex = "YearIndex";
    public const int PreviousMatches = 100;
    public const int UpcomingMatches = 3;

    // Contexts on a fixture.
    public const string IsHomeTeam = "isHomeTeam";
    public const string ShowAllEventTypes = "ShowAllEventTypes";

    // A fixture's summary. The plugin writes the game's dates as yyyy-MM-dd.
    public const string Date = "FixtureDate";
    public const string DateFormat = "yyyy-MM-dd";
    public const string Competition = "Competition.Name";
    public const string IsFriendly = "Competition.IsFriendly";
    public const string HasResult = "HasResult";
    public const string HomeTeamId = "HomeTeam." + GameConstants.UniqueId;
    public const string HomeTeamName = "HomeTeam." + GameConstants.Name;
    public const string AwayTeamId = "AwayTeam." + GameConstants.UniqueId;
    public const string AwayTeamName = "AwayTeam." + GameConstants.Name;
    public const string HomeScore = "HomeScore";
    public const string AwayScore = "AwayScore";
    public const string HomeExpectedGoals = "HomeExpectedGoals";
    public const string AwayExpectedGoals = "AwayExpectedGoals";
    public const string HomePossession = "HomePossession";
    public const string AwayPossession = "AwayPossession";
    public const string HomeShots = "HomeShots";
    public const string AwayShots = "AwayShots";
    public const string HomeShotsOnTarget = "HomeShotsOnTarget";
    public const string AwayShotsOnTarget = "AwayShotsOnTarget";
    public const string HomeLeaguePosition = "HomeLeaguePosition";
    public const string AwayLeaguePosition = "AwayLeaguePosition";

    // The user's players in the match with their position and role in one phase (context InPossession).
    public const string Lineup = "PastMatchPlayerTacticData";

    public static readonly string[] Summary =
    [
        Date, Competition, IsFriendly, HasResult, HomeTeamId, HomeTeamName, AwayTeamId, AwayTeamName, HomeScore, AwayScore,
        HomeExpectedGoals, AwayExpectedGoals, HomePossession, AwayPossession, HomeShots, AwayShots, HomeShotsOnTarget,
        AwayShotsOnTarget, HomeLeaguePosition, AwayLeaguePosition, Lineup
    ];

    // One side's numbers (context isHomeTeam).
    public const string TeamStats = "Stats";
    public const string PlayerStats = "PlayerMatchStats";

    // A player's last incident on his match line, as the game numbers them: 0 none, 1 came on, 2 went off (substituted,
    // injured or not), 3 booked (a player later sent off keeps it, with his first booking's minute), 6 came on and went
    // off injured, as seen across a season of matches.
    public const int CameOnIncident = 1;
    public const int WentOffIncident = 2;
    public const int InjuredOffIncident = 6;

    // The player of the match, and the game's post-match analysis: a list of findings, kept for the last five matches.
    public const string PlayerOfTheMatchId = "PlayerOfTheMatch." + GameConstants.UniqueId;
    public const string PlayerOfTheMatchName = "PlayerOfTheMatch." + GameConstants.Name;
    public const string PlayerOfTheMatchRating = "PlayerOfTheMatchRating";
    public const string Analysis = "PostMatchAnalysisKeyFindings";

    public static readonly string[] Events = [PlayerOfTheMatchId, PlayerOfTheMatchName, PlayerOfTheMatchRating, Analysis];

    // A match's incidents, read as a list of the fixture (context ShowAllEventTypes): goals, red cards and a shoot-out's
    // penalties, with their side (0 home, 1 away; an own goal's is the side it counts for), minute and players.
    public const string Incidents = "Incidents";
    public const string IncidentType = "IncidentType";
    public const string IncidentSide = "TeamSelectionIndex";
    public const string IncidentMinute = "TimeString";
    public const string IncidentPlayer = "Player";
    public const string IncidentPlayerName = "Player." + GameConstants.Name;
    public const string IncidentPlayerId = "Player." + GameConstants.UniqueId;
    public const string IncidentSecondPlayer = "Player2";
    public const string IncidentSecondPlayerName = "Player2." + GameConstants.Name;

    public static readonly string[] IncidentProps =
    [
        IncidentType, IncidentSide, IncidentMinute, IncidentPlayer, IncidentPlayerName, IncidentPlayerId, IncidentSecondPlayer,
        IncidentSecondPlayerName
    ];

    // Incident types, as seen across a season: a goal (the scorer, and the assister as the second player), an own goal
    // (the player who put it into his own net), a penalty scored, a red card, and a shoot-out penalty scored or missed.
    public const int GoalIncident = 0;
    public const int OwnGoalIncident = 1;
    public const int PenaltyGoalIncident = 2;
    public const int RedCardIncident = 3;
    public const int ShootoutScoredIncident = 4;
    public const int ShootoutMissedIncident = 5;

    // The players of one side's match lines, read as a list of the fixture (context IsHomeTeam), for their names: the
    // match keeps them for players who have since left the club.
    public const string PlayerStatsPlayer = "Player";
    public const string PlayerStatsName = "Player." + GameConstants.Name;
    public const string PlayerStatsId = "Player." + GameConstants.UniqueId;

    public static readonly string[] PlayerStatsPeople = [PlayerStatsPlayer, PlayerStatsName, PlayerStatsId];

    // A list belonging to the fixture at a place in the fixture list.
    public static string OfFixture(int index, string list)
    {
        return $"{FixtureList}[{index.ToString(CultureInfo.InvariantCulture)}].{list}";
    }
}
