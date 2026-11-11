using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class TacticsTools
{
    [McpServerTool(Name = "get_tactics")]
    [Description("Get the user's tactics from their running Football Manager 27 save, read live from the game without " +
                 "changing anything. Returns the three tactic slots (numbered 1-3 as in the game: name, which one is " +
                 "active, and the game's own 'tactical effectiveness' assessment of each: overall, player ability with " +
                 "role suitability and capability, player buy-in, tactical understanding by area and complexity, " +
                 "next-match outlook, the assistant's advice and any warnings), each slot's eleven positions with role " +
                 "abbreviations and the player the game would put there; and the active tactic in full: formations in " +
                 "and out of possession, mentality, style, team instructions, and per lineup slot the player, their " +
                 "position and role in each phase, their rating in that role, and their rating in every other role " +
                 "available at that position. Effectiveness scores are 0-100; role ratings are 0-20 (the game's stars, " +
                 "20 = five stars). Inactive slots are rated with the current eleven moved into that tactic by slot " +
                 "index, which is what switching would do, so a player can look misplaced there. Positions use FM's " +
                 "English codes with L/R for the side of a central line (DCL, MCR). Settings and roles come with keys " +
                 "(the game's English identifiers, as set_tactic takes them) beside the game's labels, which are in its " +
                 "display language. Fails with a 503 when the game isn't running or no save is loaded.")]
    public static async Task<string> GetTactics(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.GetAsync("/api/v1/tactics");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "get_role_fit")]
    [Description("For one position, list every role the game offers there (in or out of possession) with its key, " +
                 "the attributes each role relies on most, and the squad players the game rates best in it (0-20, the " +
                 "game's stars, 20 = five stars). Players who can't play the position at all are left out. Use it to " +
                 "find who should fill a slot, which role suits a player better, or where the squad is thin. Role and " +
                 "attribute names are in the game's display language; keys are what set_tactic takes.")]
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

    [McpServerTool(Name = "get_tactic_options")]
    [Description("List everything a tactic can be set to in the running game, read live without changing anything: " +
                 "tactical styles, mentalities, the formations the game offers for each phase, every team instruction " +
                 "with its options and the active tactic's current choice, the player instruction groups of each " +
                 "phase with their options, and every role key. Each comes as a key (the game's English identifier, " +
                 "whatever language the game runs in), which is what set_tactic takes, and the game's label. Formation " +
                 "keys are the game's internal names and don't always match the shape it shows (343DMWide is labelled " +
                 "5-2-3): go by the label. Which roles a position offers, and who suits them, comes from " +
                 "get_role_fit. Fails with a 503 when the game isn't running, or until the tactics screen has been " +
                 "opened once in the game session.")]
    public static async Task<string> GetTacticOptions(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.GetAsync("/api/v1/tactics/options");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "set_tactic")]
    [Description("Create or change a tactic in one of the user's three tactic slots in the running game, by key (see " +
                 "get_tactic_options, get_role_fit, get_tactics and get_squad). Anything left out stays as it is. " +
                 "By default this is a dry run: it checks every key and returns the planned steps without changing " +
                 "anything. Show the user the plan, and only call again with apply=true once they have agreed. " +
                 "The game applies changes in its own order: switching to the slot, style, mentality, team " +
                 "instructions, formations, then the lineup (picks, position moves, roles, player instructions), then the " +
                 "bench (substitutes); the " +
                 "first failed step stops the rest, and the answer reports each step as Done, Failed or Skipped. " +
                 "Effects to tell the user about: writing a slot makes it the tactic the team plays, and it stays " +
                 "active afterwards; a new style resets the mentality and every team instruction to that style's own " +
                 "(the ones given here are set after it); a formation change makes the game put players in other slots " +
                 "and reset roles, so give the lineup in the same call; the starting eleven is the team's selection, " +
                 "shared by all slots, so bringing players in changes it whichever tactic is played. A dry run can't " +
                 "see a slot's settings until it's active, nor positions after a formation change: those steps are " +
                 "planned at apply time. Refused while a match is being played.")]
    public static async Task<string> SetTactic(
        IHttpClientFactory httpFactory,
        [Description("The slot to write, 1 to 3. Use slot 2 unless the user asked for another; if slot 2 holds no " +
                     "tactic or the user keeps one of their own there, ask the user which slot to use.")]
        int slot,
        [Description("Tactical style key, e.g. BlankSlate or Gegenpress. Changing it resets the mentality and team " +
                     "instructions to the style's own, before the ones given here are set.")]
        string? style = null,
        [Description("Mentality key: VeryDefensive, Defensive, Cautious, Balanced, Positive, Attacking or VeryAttacking.")]
        string? mentality = null,
        [Description("Team instruction key to option key, e.g. {\"Tempo\": \"HigherTempo\", \"LineOfEngagement\": " +
                     "\"LineOfEngagementHigh\"}. Instructions not given keep their option.")]
        Dictionary<string, string>? teamInstructions = null,
        [Description("Formation keys for each phase, e.g. 433DMWide in possession and 4141DM out of it.")]
        PhaseValues? formation = null,
        [Description("Players of the starting eleven to place. To change who plays, list all eleven, each with a " +
                     "position for both phases; otherwise list only the players to change. Exactly one goalkeeper, and " +
                     "the goalkeeper's slot stays GK.")]
        List<LineupEntry>? lineup = null,
        [Description("The substitutes by FM id, in the bench's order from its first place (the bench has as many " +
                     "places as the competition allows; the answer says how many if too many are given). Players not " +
                     "in the starting eleven, nor trialists, who can't be picked. Places after the last one given keep " +
                     "whoever holds them. Left out, the bench stays as it is, and the answer warns when it's empty: " +
                     "always set it when setting up a match.")]
        List<int>? bench = null,
        [Description("False (the default) only checks and plans. True writes to the game: only once the user has " +
                     "agreed to the plan.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsJsonAsync(
            $"/api/v1/tactics?apply={apply.ToString().ToLowerInvariant()}",
            new { slot, style, mentality, teamInstructions, formation, lineup, bench });

        return await response.ReadContentOrError();
    }
}
