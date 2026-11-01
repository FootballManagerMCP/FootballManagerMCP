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

    // Goals and the player of the match (context ShowAllEventTypes).
    public const string Incidents = "Incidents";
    public const string PlayerOfTheMatchId = "PlayerOfTheMatch." + GameConstants.UniqueId;
    public const string PlayerOfTheMatchName = "PlayerOfTheMatch." + GameConstants.Name;
    public const string PlayerOfTheMatchRating = "PlayerOfTheMatchRating";
    public const int GoalIncident = 0;

    public static readonly string[] Events = [Incidents, PlayerOfTheMatchId, PlayerOfTheMatchName, PlayerOfTheMatchRating];
}
