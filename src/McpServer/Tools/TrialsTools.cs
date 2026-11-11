using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class TrialsTools
{
    [McpServerTool(Name = "offer_trials")]
    [Description("Offer players a trial at the user's club, of 1 to 4 weeks, through each player's own menu in the " +
                 "running game (\"Offer trial\"), as the user would one by one. Up to 50 players per call. Players come " +
                 "by FM id from get_scouted_players (any source: the player search's results after run_player_search, " +
                 "e.g. with the OutOfContract quick filter for free agents, sorted by reputation as the game's search " +
                 "is, the shortlist, recommendations, a focus) or search_database; for the player search, list them " +
                 "with get_scouted_players first, since only the results it has listed are reached. A free agent " +
                 "decides himself, often at once: he then joins the squad for the trial (get_squad shows him with " +
                 "trialUntil). A player under contract (an amateur at a club, say) needs his club's yes. The others " +
                 "answer over the next days. The game also adds every player it's offered to the shortlist. Players " +
                 "the game doesn't offer a trial to right now are skipped with the reason (on trial already, an offer " +
                 "waiting for his answer, on trial at another club), as are those it warns against; ids no list holds " +
                 "come back as notFound. By default a dry run that lists the players, the menu entry it would pick and " +
                 "the game's confirmation, in the game's language. Show the user how many players it covers and ask. " +
                 "To apply, call again with apply=true and expectedPlayers set to the dry run's count: if the plan has " +
                 "changed since, nothing is sent. Each player's trial state is read back after sending: OfferWaiting " +
                 "(waiting for the answer) or OnTrial (accepted at once). Applying stops at the first player the game " +
                 "refuses and says so, keeping the ones already sent. About a second per player. Refused while a " +
                 "match is being played.")]
    public static async Task<string> OfferTrials(
        IHttpClientFactory httpFactory,
        [Description("The players' FM ids (at most 50).")]
        List<int> playerIds,
        [Description("How long each trial lasts: 1, 2, 3 or 4 weeks.")]
        int weeks,
        [Description("False (the default) only checks. True sends the offers: only once the user has agreed.")]
        bool apply = false,
        [Description("Needed with apply=true: the number of players the dry run listed (its count), as the user agreed to it.")]
        int? expectedPlayers = null)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsJsonAsync(
            $"/api/v1/trials?apply={apply.ToString().ToLowerInvariant()}",
            new { playerIds, weeks, expectedPlayers });

        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "end_trials")]
    [Description("Let players go from a trial, through each player's own menu in the running game, as the user would " +
                 "one by one: for a player on trial at the club, \"End trial\" (he leaves the club shortly after); for " +
                 "one who hasn't answered the offer yet, the offer is withdrawn. Up to 50 players per call, by FM id: " +
                 "trialists from get_squad (those with trialUntil), or players offered a trial with offer_trials. " +
                 "Judge trialists first, with the user: get_squad with includeAttributes (attributes become known as " +
                 "the staff watch them train) and get_role_fit against the club's own players. Players neither on " +
                 "trial nor waiting on an offer are skipped with the reason. By default a dry run that lists each " +
                 "player, the menu entry it would pick (ending the trial, or withdrawing the offer: check it's the " +
                 "trial's, since the game words a transfer offer's withdrawal the same way) and the game's " +
                 "confirmation, in the game's language. Show the user who goes and ask. To apply, call again with " +
                 "apply=true and expectedPlayers set to the dry run's count: if the plan has changed since, nothing is " +
                 "sent. Each player's trial state is read back after sending. Applying stops at the first player the " +
                 "game refuses and says so, keeping the ones already done. About a second per player. Refused while a " +
                 "match is being played.")]
    public static async Task<string> EndTrials(
        IHttpClientFactory httpFactory,
        [Description("The players' FM ids (at most 50).")]
        List<int> playerIds,
        [Description("False (the default) only checks. True lets them go: only once the user has agreed.")]
        bool apply = false,
        [Description("Needed with apply=true: the number of players the dry run listed (its count), as the user agreed to it.")]
        int? expectedPlayers = null)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsJsonAsync(
            $"/api/v1/trials/end?apply={apply.ToString().ToLowerInvariant()}",
            new { playerIds, expectedPlayers });

        return await response.ReadContentOrError();
    }
}
