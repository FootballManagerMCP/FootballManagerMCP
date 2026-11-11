using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class DatabaseTools
{
    [McpServerTool(Name = "search_database")]
    [Description("Search the whole Football Manager 27 database with the game's own search box, as the user would: " +
                 "players, staff, clubs, national teams, nations and competitions anywhere in the world, not only " +
                 "those the user's scouting knows. Returns the results from the closest match down, each with its kind " +
                 "(Player, Staff, Club, NationalTeam, Nation, Competition, Other), its FM id and name, the game's line " +
                 "under the name (a player's position and club or 'Sans contrat', a staff member's job, a club's " +
                 "standing), and by kind: a person's age, nationality, club (with its id and nation) and league, a " +
                 "player's positions, a staff member's job; a club's full name, nation and league; a competition's " +
                 "nation; a national team's type (men's or women's), nation and continent; a nation's continent. " +
                 "Also how many results the game found. The search is fuzzy: past the close matches come loose ones " +
                 "(searching 'Ferry' also finds 'Kerry' and 'Harry Winks'), so check the names. " +
                 "Names are in the game's display language ('Écosse' in a French game), and clubs without a licence go " +
                 "by a short code (Corinthians is 'COR'). It types the text into the game's search box, which shows " +
                 "it afterwards; nothing else changes. The ids found work in the other tools: a club's (or a person's " +
                 "clubId) in get_team, a player's in scout_player and shortlist_player. Fails with a 503 when the game " +
                 "isn't running.")]
    public static async Task<string> SearchDatabase(
        IHttpClientFactory httpFactory,
        [Description("A name or part of one: 'Memphis Depay', 'Valenciennes'. Accents and case don't matter.")]
        string query,
        [Description("Only these kinds: Player, Staff, Club, NationalTeam, Nation, Competition, Other. All kinds when " +
                     "left out. Only the first 200 results are looked through for them.")]
        List<string>? kinds = null,
        [Description("At most this many results (1-50), 10 by default. A result is about 200 bytes.")]
        int? limit = null)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var parameters = new List<string> { $"query={Uri.EscapeDataString(query)}" };
        parameters.AddRange((kinds ?? []).Select(kind => $"kinds={Uri.EscapeDataString(kind)}"));

        if (limit is { } max)
        {
            parameters.Add($"limit={max}");
        }

        var response = await http.GetAsync($"/api/v1/database/search?{string.Join('&', parameters)}");
        return await response.ReadContentOrError();
    }
}
