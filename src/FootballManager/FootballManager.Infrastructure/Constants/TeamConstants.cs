namespace FootballManager.Infrastructure.Constants;

// Paths to other teams. The game has no lookup by id, so a team is reached through a list holding it: the user's
// league, or a competition of one of the user's fixtures. The plugin's selectors pick a list item by index
// ("Club.MainTeam.Division.TeamList[3]"), cheaper than by property since it reads no item; a competition's team list
// keeps its order whatever team is passed. Paths start from the club's main team, not Human.Team, which reading
// another team switches (see TeamContext).
internal static class TeamConstants
{
    public const string OwnTeam = "Club.MainTeam";
    public const string LeagueTeams = OwnTeam + ".Division.TeamList";
    public const string LeagueName = OwnTeam + ".Division.Name";
    public const string OwnFixtures = OwnTeam + "." + FixtureList;
    public const string CompetitionTeams = "Competition.TeamList";

    // On a team record.
    public const string FixtureList = "FixtureList";
    public const string Players = "Players";
    public const string DivisionName = "Division.Name";

    // A club's first team, for a club found with the game's search (on the game root).
    public const string MainTeam = "MainTeam";

    // The team a past match's lineup is read for (PastMatchPlayerTacticData), as the game's screens pass it. Passing a
    // team this way makes it the user's current team (Human.Team, and with it the active tactic) until another team is
    // passed: after reading another team, the user's own goes back, from the club's main team, which doesn't change.
    public const string TeamContext = "team";

    public static string Item(string listPath, int index)
    {
        return $"{listPath}[{index}]";
    }
}
