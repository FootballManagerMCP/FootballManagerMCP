using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class TeamsTools
{
    [McpServerTool(Name = "get_team")]
    [Description("Get a scouting report on any team of the user's league, or of a competition the user plays in this " +
                 "season (European competitions, cups), read live from the game. Returns, from the team's competitive " +
                 "matches: its record overall, home and away (results, goals, xG); its style per match overall, at " +
                 "home and away, both 'for' (how it plays) and 'against' (what its opponents manage against it): " +
                 "possession, xG, shots, long shots, chances created, crosses and their completion, passes and " +
                 "completion, progressive and final-third passes, PPDA (opposition passes per defensive action: the " +
                 "lower, the harder it presses), tackles, headers won and fouls; how it lines up: its shapes in and out " +
                 "of possession with how often each was used, and its regular starters with their usual position and " +
                 "role keys in both phases; its squad, as far as the user's scouting knows it; every competitive match " +
                 "from its side with the shapes it played; and its next fixtures. Use it to scout an opponent before " +
                 "setting up a tactic. Some results the game only simulated have no numbers, so averages can cover " +
                 "fewer matches than the record. Team and player names are in the game's display language. Reading a " +
                 "team briefly makes it the game's current team, and the user's own is put back at the end: don't run " +
                 "it alongside set_tactic. Fails with a 503 when the game isn't running.")]
    public static async Task<string> GetTeam(
        IHttpClientFactory httpFactory,
        [Description("The team's name as the game writes it, in its display language (e.g. 'Barcelone' in a French " +
                     "game); case and accents don't matter, and part of the name works when it's unambiguous. Or the " +
                     "team's FM id. An unknown name returns the closest teams with their ids.")]
        string team)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.GetAsync($"/api/v1/teams/{Uri.EscapeDataString(team)}");
        return await response.ReadContentOrError();
    }
}
