using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class CompetitionsTools
{
    [McpServerTool(Name = "get_competitions")]
    [Description("List the leagues whose stats screens get_competition_stats can read, live from the user's running " +
                 "Football Manager 27 save: the user's league first, then every league the game plays out in the " +
                 "nations of the user's continent, each with its FM id, nation and the game's reputation for it (the " +
                 "higher, the stronger: compare with the user's league to find leagues of a similar level). Names are " +
                 "in the game's display language. This only reads.")]
    public static async Task<string> GetCompetitions(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.GetAsync("/api/v1/competitions");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "get_competition_stats")]
    [Description("Rank a league's players by their numbers this season, from the competition's stats screen in the " +
                 "user's running Football Manager 27 save, and cross-check each one: the way to find players who did " +
                 "well in the user's league or a similar one. The game keeps each stat's top 100 players; the players " +
                 "come from the sortBy stat's top 100 (best first, as the game ranks it), narrowed down by the " +
                 "filters, and each also gets his value and rank in the other stats read where he's in their top 100 " +
                 "(left out otherwise). For each player: his club's team id, positions (FM's English codes), minutes " +
                 "for his club this season (all competitions), the stats; what the user's scouting " +
                 "knows of him (as get_scouted_players gives it: age, club, nation, the scouts' ability and potential " +
                 "stars on the 0-20 scale, grade and verdict, knowledge level, value and wage ranges, statuses such as " +
                 "amateur, transfer listed, unhappy); and his contract: its kind (amateur, part-time, full-time), its " +
                 "term (monthly rolling or fixed), end date (none for amateur terms), wage with its period, the game's " +
                 "value estimate, asking price, transfer status. Amateurs on monthly terms can be approached without a " +
                 "fee. Stars stay unknown until a player is scouted (scout_player). Per90 stats are the game's own; " +
                 "percentages run 0-100. The tables cover this season only and likely reset when the next one starts. " +
                 "This only reads.")]
    public static async Task<string> GetCompetitionStats(
        IHttpClientFactory httpFactory,
        [Description("The stats to read, 1 to 10. General: Appearances, Starts, Minutes, AverageRating, " +
                     "PlayerOfTheMatch, MatchesWon, MatchesLost, YellowCards, RedCards, Offsides(Per90), " +
                     "DistanceCovered(Per90), Sprints(Per90). Scoring: Goals(Per90), MinutesPerGoal, PenaltiesScored, " +
                     "PenaltyConversionPercentage, FreeKickGoals(Per90), Shots(Per90), ShotsOnTarget(Per90), " +
                     "ShotsOnTargetPercentage, ConversionPercentage, ExpectedGoals(Per90), NonPenaltyExpectedGoals(Per90), " +
                     "ExpectedGoalsPerShot, ExpectedGoalsOverperformance, ExpectedGoalsUnderperformance. Creating: " +
                     "Assists(Per90), ExpectedAssists(Per90), OpenPlayExpectedAssists(Per90), SetPieceExpectedAssists, " +
                     "KeyPasses(Per90), OpenPlayKeyPasses(Per90), ChancesCreated(Per90), Dribbles(Per90), " +
                     "CrossesAttempted(Per90), CrossesCompleted(Per90), CrossCompletionPercentage, " +
                     "OpenPlayCrossesAttemptedPer90, OpenPlayCrossesCompletedPer90, SetPieceCrossesAttempted. Passing: " +
                     "PassesAttempted(Per90), PassesCompleted(Per90), PassCompletionPercentage, ProgressivePasses(Per90). " +
                     "Defending: TacklesAttempted(Per90), TacklesWon(Per90), TackleSuccessPercentage, KeyTackles(Per90), " +
                     "Interceptions(Per90), PossessionWon(Per90), PossessionLost(Per90), Blocks(Per90), " +
                     "ShotsBlocked(Per90), Clearances(Per90), PressuresAttempted, PressuresCompleted, Fouls(Per90), " +
                     "FoulsWon(Per90), Errors, ErrorsLeadingToGoal. In the air: AerialDuels(Per90), HeadersWon(Per90), " +
                     "HeaderWinPercentage, HeadersLost(Per90), KeyHeaders(Per90). Goalkeeping: GoalsConceded(Per90), " +
                     "CleanSheets(Per90), Saves(Per90), SavesHeld, SavesParried, SavesTipped, SavePercentage, " +
                     "ExpectedSavePercentage, ExpectedSavesPer90, ExpectedGoalsPrevented(Per90), " +
                     "ExpectedGoalsConcededPer90, PenaltiesFaced, PenaltiesSaved, PenaltySavePercentage. " +
                     "'(Per90)' means both the total and the Per90 key exist, e.g. TacklesWon and TacklesWonPer90.")]
        List<string> stats,
        [Description("The stat whose top 100 players are picked from and ranked by; the first of stats when left out. " +
                     "Rank by a per-90 or percentage stat with minMinutes set, so short spells don't lead.")]
        string? sortBy = null,
        [Description("The league's name as the game writes it, or its FM id, from get_competitions; the user's league " +
                     "when left out.")]
        string? competition = null,
        [Description("Only players who can play one of these positions, as FM's English codes: GK, DR, DC, DL, WBR, " +
                     "WBL, DM, MR, MC, ML, AMR, AMC, AML, ST.")]
        List<string>? positions = null,
        int? minAge = null,
        int? maxAge = null,
        [Description("Only players with at least this many minutes for their club this season.")]
        int? minMinutes = null,
        [Description("Also list the user's own players, to compare them with the league (left out by default).")]
        bool includeOwnPlayers = false,
        [Description("How many players to return, 1 to 50 (25 by default). Each is read in full, about 1 KB.")]
        int limit = 25)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var query = new List<string>();
        query.AddRange(stats.Select(stat => $"stats={Uri.EscapeDataString(stat)}"));
        query.AddRange((positions ?? []).Select(position => $"positions={Uri.EscapeDataString(position)}"));
        Add(query, "sortBy", sortBy);
        Add(query, "competition", competition);
        Add(query, "minAge", minAge?.ToString());
        Add(query, "maxAge", maxAge?.ToString());
        Add(query, "minMinutes", minMinutes?.ToString());
        Add(query, "includeOwnPlayers", includeOwnPlayers.ToString().ToLowerInvariant());
        Add(query, "limit", limit.ToString());

        var response = await http.GetAsync($"/api/v1/competitions/player-stats?{string.Join('&', query)}");
        return await response.ReadContentOrError();
    }

    private static void Add(List<string> query, string name, string? value)
    {
        if (value is not null)
        {
            query.Add($"{name}={Uri.EscapeDataString(value)}");
        }
    }
}
