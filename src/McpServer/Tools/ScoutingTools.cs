using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class ScoutingTools
{
    [McpServerTool(Name = "get_scouting")]
    [Description("Get the user's player recruitment set-up from their running Football Manager 27 save, read live from " +
                 "the game: how many players the scouting centre recommends (and how many highly), reports still being " +
                 "written, the shortlist's size and how many players the club's scouting knows; the club's scouts with " +
                 "their judging player ability and potential (1-20); and every recruitment focus (the standing briefs " +
                 "the scouts work to) with its id, name, status, the areas and scouts covering it, who created it and " +
                 "when, its age range, the minimum current and potential ability stars it asks for (0-20 scale, 20 = " +
                 "five stars), its positions (none listed means any), how many players it recommends, is still " +
                 "assessing and narrowly rejected, and the scouts' comment on how it's going. Use the focus ids with " +
                 "get_scouted_players. A focus marked showsRemovedFocus is the game still showing a focus removed " +
                 "earlier under an id it has since given a newer focus: its details and players are the removed one's " +
                 "until the game refreshes its list. Text is in the game's display language. Fails with a 503 when the " +
                 "game isn't running or no save is loaded.")]
    public static async Task<string> GetScouting(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.GetAsync("/api/v1/scouting");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "get_scouted_players")]
    [Description("Get players from one of the user's scouting lists, as the club's scouting knows them (the game's fog " +
                 "of war: ranges where knowledge is partial, nothing the user's scouts haven't found out), best graded " +
                 "first. By default the scouting centre's recommended players; 'search' reads the results of the game's " +
                 "player search, which runs over the whole database with the filters its search screen holds (set in the " +
                 "game or with run_player_search), with the screen's status, interest and attribute filter, what it " +
                 "filters on (quick filters, interest type and level, and the rules switched on with the game's labels; " +
                 "otherConditions counts conditions added in the game's filter editor that aren't listed), and total, " +
                 "the number of players it found. For each: FM id, name, age, club, " +
                 "nation, positions, the scouts' best position and role in and out of possession (the game's English " +
                 "role keys, as get_role_fit and set_tactic use them), grade (0 'needs watching' to 12 'priority', " +
                 "with the game's label) and verdict, current and potential ability stars (0-20 scale, 20 = five " +
                 "stars, relative to the user's squad; a range like '17-20' while knowledge is partial), knowledge " +
                 "level (out of 5), estimated value and wage, strongest and weakest areas, whether he's " +
                 "shortlisted or being scouted, and the statuses the game shows for him (its status icons), each with " +
                 "the game's English key, label and details: FutureTransfer (a transfer agreed, with the club and " +
                 "date he joins), TransferListed and ListedForLoan (with the asking price), Wanted (the clubs watching " +
                 "him), injured, free agent, work permit, and so on. Text values are in the game's display language. " +
                 "Fails with a 503 when the game isn't running or no save is loaded.")]
    public static async Task<string> GetScoutedPlayers(
        IHttpClientFactory httpFactory,
        [Description("Which list: 'recommendations' (the scouting centre's, across every focus), 'shortlist' (the " +
                     "user's shortlist), 'focus' (one recruitment focus's players; needs focusId) or 'search' (the game's " +
                     "player search results, in the search screen's order; often hundreds of players, mostly unscouted).")]
        string source = "recommendations",
        [Description("The recruitment focus's id, from get_scouting. Only with source 'focus'.")]
        int? focusId = null,
        [Description("For a focus: 'recommended' (the players its scouts recommend), 'current' (players its scouts " +
                     "have found and are still assessing) or 'nearMiss' (players that narrowly missed its criteria).")]
        string pool = "recommended",
        [Description("Include each player's visible attributes (1-20), grouped as technical, mental, physical and " +
                     "goalkeeping; a range like '11-14' means scouting is incomplete. About 1.5 KB more per player.")]
        bool includeAttributes = false,
        [Description("Include hidden data: personality attributes (professionalism, ambition, loyalty, pressure, " +
                     "temperament, ...) and current/potential ability. FM keeps these hidden on purpose and seeing them " +
                     "changes how the game is played, so you MUST ask the user and get their explicit agreement before " +
                     "setting this to true. Without FM's In-Game Editor the game masks them: they come back null and " +
                     "hiddenDataMasked is true.")]
        bool includeHidden = false,
        [Description("Include each player's latest scout report: its date and scout, the scout's view of his level and " +
                     "potential, his pros and cons (personality, fit with the club's vision, release clauses, ...), the " +
                     "fee and wage he'd likely cost, and how interested he'd be in a transfer or a loan.")]
        bool includeReport = false,
        [Description("At most this many players (1-200). The search reads its first 50 by default (about a second " +
                     "per 30 players); the other lists are short and come whole unless this is set.")]
        int? limit = null)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var query = $"source={Uri.EscapeDataString(source)}" +
                    (focusId is { } id ? $"&focusId={id}" : string.Empty) +
                    $"&pool={Uri.EscapeDataString(pool)}" +
                    $"&includeAttributes={includeAttributes.ToString().ToLowerInvariant()}" +
                    $"&includeHidden={includeHidden.ToString().ToLowerInvariant()}" +
                    $"&includeReport={includeReport.ToString().ToLowerInvariant()}" +
                    (limit is { } max ? $"&limit={max}" : string.Empty);

        var response = await http.GetAsync($"/api/v1/scouting/players?{query}");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "run_player_search")]
    [Description("Run the game's player search (its search screen, over the whole player database) with these " +
                 "filters, in the user's running game. It replaces the rules and quick filters the user's search screen " +
                 "holds: whatever isn't given is switched off, conditions added by hand in the game's filter editor " +
                 "(nationality, value, ...) included. The interest setting only changes when given. By default a dry " +
                 "run: it checks every value against the game and returns what the search holds now (before) and the " +
                 "plan, changing nothing. Tell the user what their search held, and only call again with apply=true " +
                 "once they have agreed; the answer then has the search as the game holds it (its filters read back) " +
                 "and total, how many players it found (the game lists at most 10000). Read the players with " +
                 "get_scouted_players, source 'search'. Refused while a match is being played.")]
    public static async Task<string> RunPlayerSearch(
        IHttpClientFactory httpFactory,
        [Description("One-click filters to switch on: TransferListed, ListedForLoan, OutOfContract (free agents), " +
                     "ContractExpiring (at a club, contract running out), HomeGrown, Unhappy, TwoFooted, Goalkeepers, " +
                     "Defenders, Midfielders, Attackers, HighKnowledge (players the club's scouting knows well), " +
                     "NoWorkPermitNeeded.")]
        List<string>? quickFilters = null,
        [Description("The youngest age, 15 to 45.")]
        int? minAge = null,
        [Description("The oldest age, 15 to 45.")]
        int? maxAge = null,
        [Description("Positions he must play, any of them: GK, DR, DC, DL, WBR, WBL, DM, MR, MC, ML, AMR, AMC, AML, ST. " +
                     "Leave out for any position.")]
        List<string>? positions = null,
        [Description("How well he must play them: Natural (the default) or Accomplished, Competent, each with the " +
                     "levels above it; Able (can play there at all) or Unable.")]
        string? positionFamiliarity = null,
        [Description("The scouts' grade he needs at least: A+, A, A-, B+, B, B-, C+, C, C-, D, E or F. Only players " +
                     "the club's scouts have graded pass.")]
        string? minScoutingGrade = null,
        [Description("Which move his interest is judged for: TransferOrLoan, TransferOnly, LoanOnly, or NotConsidered " +
                     "(interest left out). Unchanged when left out.")]
        string? interestType = null,
        [Description("How interested he must at least be in it: Interested, SlightInterest (can be persuaded) or " +
                     "Doubtful. Unchanged when left out.")]
        string? interestLevel = null,
        [Description("False (the default) only checks and plans. True runs the search, replacing the user's filters: " +
                     "only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PutAsJsonAsync(
            $"/api/v1/scouting/search?apply={apply.ToString().ToLowerInvariant()}",
            new { quickFilters, minAge, maxAge, positions, positionFamiliarity, minScoutingGrade, interestType, interestLevel });

        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "create_recruitment_focus")]
    [Description("Create a recruitment focus in the user's running game, as the game's own focus wizard does: a " +
                 "standing brief for the club's scouts, who then look for players matching it and recommend them " +
                 "(the game assigns scouts to it itself). By default this is a dry run: it checks every value against " +
                 "what the game offers and returns the plan, changing nothing. Show the user the plan, and only call " +
                 "again with apply=true once they have agreed; the answer then has the focus as the game holds it. " +
                 "Unknown or ambiguous areas come back with the matching ones; get_scouting lists the existing " +
                 "focuses. Refused while a match is being played.")]
    public static async Task<string> CreateRecruitmentFocus(
        IHttpClientFactory httpFactory,
        [Description("The focus's name, as the user will see it in the game; it must differ from the club's other focuses.")]
        string name,
        [Description("Where to scout, by name in the game's display language (e.g. 'Espagne', 'Europe centrale', " +
                     "'Amérique du S.'), each a competition, nation, region or continent. Names repeat across kinds " +
                     "(a nation and a continent, a region and its continent): prefix the kind to pick one, e.g. " +
                     "'nation:Espagne', 'region:Europe centrale', 'continent:Europe', 'competition:Ligue 1', or add its " +
                     "group in brackets, e.g. 'Premiership (Écosse)'.")]
        List<string>? areas = null,
        [Description("Position codes to look for: GK, DR, DCR, DC, DCL, DL, WBR, DMR, DM, DML, WBL, MR, MCR, MC, MCL, " +
                     "ML, AMR, AMCR, AMC, AMCL, AML, STR, ST, STL. Leave out for any position.")]
        List<string>? positions = null,
        [Description("The youngest age to look at (15 or more); the game's default otherwise.")]
        int? minAge = null,
        [Description("The oldest age to look at; the game's default otherwise.")]
        int? maxAge = null,
        [Description("The current ability stars a player needs, from 1 (half a star) to 20 (five stars), relative to " +
                     "the user's squad; the game's default otherwise. Can't be above minPotentialStars.")]
        int? minAbilityStars = null,
        [Description("The potential ability stars a player needs, on the same scale; the game's default otherwise.")]
        int? minPotentialStars = null,
        [Description("False (the default) only checks and plans. True creates the focus: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsJsonAsync(
            $"/api/v1/scouting/focuses?apply={apply.ToString().ToLowerInvariant()}",
            new { name, areas, positions, minAge, maxAge, minAbilityStars, minPotentialStars });

        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "remove_recruitment_focus")]
    [Description("Remove one of the club's recruitment focuses in the running game, as the scouting screen's remove " +
                 "does: its scouts stop working on it. By default a dry run that names the focus; call again with " +
                 "apply=true once the user has agreed. Refused while a match is being played.")]
    public static async Task<string> RemoveRecruitmentFocus(
        IHttpClientFactory httpFactory,
        [Description("The focus's id, from get_scouting.")]
        int focusId,
        [Description("False (the default) only checks. True removes the focus: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.DeleteAsync($"/api/v1/scouting/focuses/{focusId}?apply={apply.ToString().ToLowerInvariant()}");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "scout_player")]
    [Description("Send a scout to watch a player, through the player's own menu in the running game (as the user " +
                 "would right-click him): a named scout or other member of staff, or any scout. The player must be in " +
                 "one of the user's scouting lists (get_scouted_players: the recommendations, the shortlist, a focus's " +
                 "players, the player search's results), or have been found with search_database. By default a dry run that returns the menu entry it would pick, " +
                 "in the game's language; " +
                 "call again with apply=true once the user has agreed. Scouting costs money from the scouting budget, " +
                 "more for longer or farther assignments. An unknown name returns everyone the game offers. Refused " +
                 "while a match is being played.")]
    public static async Task<string> ScoutPlayer(
        IHttpClientFactory httpFactory,
        [Description("The player's FM id, from get_scouted_players or search_database.")]
        int playerId,
        [Description("Who should watch him: a scout's name as get_scouting lists it (the start of it is enough, e.g. " +
                     "'Mark Cooper'). The game also offers the director of football and some coaches. Leave out for " +
                     "any scout.")]
        string? scout = null,
        [Description("How long: UntilFullKnowledge (the default), OneWeek, TwoWeeks, OneMonth, TwoMonths, ThreeMonths, " +
                     "OneMatch, TwoMatches, ThreeMatches or FourMatches.")]
        string duration = "UntilFullKnowledge",
        [Description("False (the default) only checks. True sends the scout: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsJsonAsync(
            $"/api/v1/scouting/players/{playerId}/scout?apply={apply.ToString().ToLowerInvariant()}",
            new { scout, duration });

        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "shortlist_player")]
    [Description("Add a player to the user's shortlist, or take him off it, through the player's own menu in the " +
                 "running game. The player must be in one of the user's scouting lists, the player search's results " +
                 "included (get_scouted_players), or have been found with search_database. By " +
                 "default a dry run that returns the menu entry it would pick; call again with apply=true once the " +
                 "user has agreed. Refused while a match is being played.")]
    public static async Task<string> ShortlistPlayer(
        IHttpClientFactory httpFactory,
        [Description("The player's FM id, from get_scouted_players or search_database.")]
        int playerId,
        [Description("True to take him off the shortlist instead of adding him.")]
        bool remove = false,
        [Description("When adding, how long he stays: Indefinitely (the default), OneMonth, ThreeMonths, SixMonths or OneYear.")]
        string duration = "Indefinitely",
        [Description("False (the default) only checks. True changes the shortlist: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var path = $"/api/v1/scouting/players/{playerId}/shortlist?apply={apply.ToString().ToLowerInvariant()}";

        var response = remove
            ? await http.DeleteAsync(path)
            : await http.PostAsJsonAsync(path, new { duration });

        return await response.ReadContentOrError();
    }
}
