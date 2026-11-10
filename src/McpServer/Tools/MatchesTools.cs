using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class MatchesTools
{
    [McpServerTool(Name = "get_matches")]
    [Description("Get the user's season so far from their running Football Manager 27 save, read live from the game. " +
                 "Returns the record over competitive matches (played, won, drawn, lost, goals and xG for and " +
                 "against), overall and split home/away; every match played in date order; and the next few fixtures. " +
                 "Each match reads from the user's side: date, competition, venue, opponent and their current league " +
                 "position, outcome, goals, xG, possession, shots and shots on target for and against, and the shape " +
                 "the user's team lined up in with the ball (e.g. '4-2-2-2'), which tells which of their tactics was " +
                 "used. Use get_match for one game in depth. Competition names and league positions are in the game's " +
                 "display language. Fails with a 503 when the game isn't running or no save is loaded.")]
    public static async Task<string> GetMatches(
        IHttpClientFactory httpFactory,
        [Description("Also list friendlies among the matches played. They never count in the record.")]
        bool includeFriendlies = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.GetAsync($"/api/v1/matches?includeFriendlies={includeFriendlies.ToString().ToLowerInvariant()}");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "get_match")]
    [Description("Get one of the user's matches in depth: the summary (as in get_matches), both teams' numbers (xG, " +
                 "possession, shots, chances created, crosses, passes, progressive and final-third passes, PPDA, " +
                 "tackles, headers, offsides, cards, average rating), the goals with minute, scorer and assister, the " +
                 "player of the match, the user's substitutions (minute, who came off and on, the slot's position, " +
                 "injuries), and the user's players who took part: rating, when a substitute came on and when a player " +
                 "went off (and whether injured), cards, position and role in and out of possession, and their numbers " +
                 "(goals, assists, xG, xA, shots, key passes, clear-cut chances, passes, dribbles, crosses, tackles, " +
                 "interceptions, possession won and lost, aerial duels, distance). A substitute's positions and roles " +
                 "are those of the slot he took: the game doesn't keep any reshuffle made after a change. The game " +
                 "doesn't link a player's slots in the two phases either: out-of-possession positions are paired with " +
                 "the starters so that the eleven move least, which matches the tactic's own links. A substitute who " +
                 "came on and went off again is traced when only one fits, else his minute on is left out. Names are " +
                 "only known for the user's own players. An upcoming match returns just its summary. Roles come as " +
                 "keys (the game's English identifiers, as set_tactic takes them) and names in the game's display " +
                 "language; the name is missing for a player in a position he can't play.")]
    public static async Task<string> GetMatch(
        IHttpClientFactory httpFactory,
        [Description("The match date as yyyy-MM-dd, as get_matches lists it.")]
        string date)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.GetAsync($"/api/v1/matches/{Uri.EscapeDataString(date)}");
        return await response.ReadContentOrError();
    }
}
