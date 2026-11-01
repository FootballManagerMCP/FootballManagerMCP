using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class SquadTools
{
    [McpServerTool(Name = "get_squad")]
    [Description("Get the user's first-team squad from their running Football Manager 27 save, read live from the game. " +
                 "For each player: FM id, name, age, positions, best role, personality, morale, condition (%, absent " +
                 "while injured), match sharpness (%), injury status and average rating this season. Optionally each " +
                 "player's visible attributes (1-20), grouped as technical, mental, physical and goalkeeping; a range " +
                 "like '11-14' means scouting is incomplete. Text values (positions, roles, morale, personality) are in " +
                 "the game's display language. Fails with a 503 when the game isn't running or no save is loaded.")]
    public static async Task<string> GetSquad(
        IHttpClientFactory httpFactory,
        [Description("Include every player's visible attributes. A much larger answer (around 20 KB for a full squad), " +
                     "so only ask for it when attributes matter, e.g. picking a lineup or judging a player.")]
        bool includeAttributes = false,
        [Description("Include hidden data: personality attributes (professionalism, ambition, loyalty, pressure, " +
                     "temperament, ...) and current/potential ability. FM keeps these hidden on purpose and seeing them " +
                     "changes how the game is played, so you MUST ask the user and get their explicit agreement before " +
                     "setting this to true. Without FM's In-Game Editor the game masks them: they come back null and " +
                     "hiddenDataMasked is true.")]
        bool includeHidden = false,
        [Description("Include each player's numbers this season, all competitive matches together: appearances, " +
                     "minutes, goals, assists, xG, non-penalty xG, xG overperformance (goals minus xG; below zero means " +
                     "finishing below the chances' worth), xG per shot (chance quality), xA, shots per 90, shots on " +
                     "target, key passes, chances created and pass completion.")]
        bool includeStats = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var query = $"includeAttributes={includeAttributes.ToString().ToLowerInvariant()}" +
                    $"&includeHidden={includeHidden.ToString().ToLowerInvariant()}" +
                    $"&includeStats={includeStats.ToString().ToLowerInvariant()}";

        var response = await http.GetAsync($"/api/v1/squad?{query}");
        return await response.ReadContentOrError();
    }
}
