using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class TacticsTools
{
    [McpServerTool(Name = "get_tactics")]
    [Description("Get the user's tactics from their running Football Manager 27 save, read live from the game without " +
                 "changing anything. Returns the three tactic slots (name, which one is active, and the game's own " +
                 "'tactical effectiveness' assessment of each: overall, player ability with role suitability and " +
                 "capability, player buy-in, tactical understanding by area and complexity, next-match outlook, the " +
                 "assistant's advice and any warnings), each slot's eleven positions with role abbreviations and the " +
                 "player the game would put there; and the active tactic in full: formations in and out of possession, " +
                 "mentality, style, team instructions, and per lineup slot the player, their position and role in each " +
                 "phase, their rating in that role, and their rating in every other role available at that position. " +
                 "Effectiveness scores are 0-100; role ratings are 0-20 (the game's stars, 20 = five stars). Inactive " +
                 "slots are rated with the current eleven moved into that tactic by slot index, which is what switching " +
                 "would do, so a player can look misplaced there. Positions use FM's English codes with L/R for the " +
                 "side of a central line (DCL, MCR); role names, advice and position labels are in the game's display " +
                 "language. Fails with a 503 when the game isn't running or no save is loaded.")]
    public static async Task<string> GetTactics(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.GetAsync("/api/v1/tactics");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "get_role_fit")]
    [Description("For one position, list every role the game offers there (in or out of possession), the attributes " +
                 "each role relies on most, and the squad players the game rates best in it (0-20, the game's stars, " +
                 "20 = five stars). Players who can't play the position at all are left out. Use it to find who should " +
                 "fill a slot, which role suits a player better, or where the squad is thin. Role and attribute names " +
                 "are in the game's display language.")]
    public static async Task<string> GetRoleFit(
        IHttpClientFactory httpFactory,
        [Description("FM's position code: GK, SW, DR, DL, DC, WBR, WBL, DM, MR, ML, MC, AMR, AML, AMC or ST. For a " +
                     "central slot like DCL or MCR, use the code without the side (DC, MC).")]
        string position,
        [Description("True for the in-possession roles, false for the out-of-possession ones.")]
        bool inPossession = true,
        [Description("How many players to list per role, best first (1-40).")]
        int top = 5)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var query = $"position={Uri.EscapeDataString(position)}" +
                    $"&inPossession={inPossession.ToString().ToLowerInvariant()}" +
                    $"&top={top}";

        var response = await http.GetAsync($"/api/v1/tactics/role-fit?{query}");
        return await response.ReadContentOrError();
    }
}
