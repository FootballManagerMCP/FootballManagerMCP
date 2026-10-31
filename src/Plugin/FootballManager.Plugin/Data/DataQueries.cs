namespace FootballManager.Plugin.Data;

// Reads game data through PropertyRequest. The squad comes from the user's manager ("Human" root), so no screen
// needs to be open.
internal static class DataQueries
{
    public const string HumanRoot = "Human";
    public const string SquadPath = "Team.Players";

    public static readonly string[] DefaultPlayerProperties =
    [
        "Name", "Age", "UniqueId", "PositionCombinedString", "Nation.Name", "Condition", "Sharpness", "IsInjured",
        "MoraleString", "BestRole", "AverageRating"
    ];

    public static async Task<string> GetAsync(
        string root, IReadOnlyList<string> paths, bool withContexts, TimeSpan timeout, bool debug)
    {
        var result = await MainThreadDispatcher.RunAcrossFramesAsync(
            () => PropertyRequest.Start(GameBindings.ValueOf(GameBindings.FindRoot(root)), paths, Contexts(withContexts)),
            request => request.IsComplete,
            request => request.Finish(debug),
            timeout);

        return Json.Serialize(result);
    }

    public static async Task<string> PlayersAsync(
        IReadOnlyList<string> paths, int? index, bool withContexts, TimeSpan timeout, bool debug)
    {
        var players = await MainThreadDispatcher.RunAcrossFramesAsync(
            () => PropertyRequest.Start(GameBindings.ValueOf(GameBindings.FindRoot(HumanRoot)), [SquadPath], [], SquadPath),
            request => request.IsComplete,
            request => request.FinishWithList(SquadPath),
            timeout);

        if (index is not null)
        {
            if (index < 0 || index >= players.Count)
            {
                throw new ArgumentException($"The squad has {players.Count} players; index must be 0 to {players.Count - 1}.");
            }

            players = [players[index.Value]];
        }

        var rows = await MainThreadDispatcher.RunAcrossFramesAsync(
            () => PropertyRequest.StartAll(players, paths, Contexts(withContexts)),
            requests => requests.All(r => r.IsComplete),
            requests => FinishAll(requests, debug),
            timeout);

        return Json.Serialize(new Dictionary<string, object?> { ["count"] = rows.Count, ["players"] = rows });
    }

    // The user's manager and team, which is what the game's own screens pass for "human" and "team".
    private static IReadOnlyList<RequestContext> Contexts(bool withContexts)
    {
        if (!withContexts)
        {
            return [];
        }

        var human = GameBindings.FindRoot(HumanRoot);
        var contexts = new List<RequestContext>();
        var humanValue = GameBindings.ValueOf(human);

        if (humanValue is not null)
        {
            contexts.Add(new RequestContext("human", humanValue));
        }

        var team = GameBindings.FindChild(human, "Team");
        var teamValue = team is null ? null : GameBindings.ValueOf(team);

        if (teamValue is not null)
        {
            contexts.Add(new RequestContext("team", teamValue));
        }

        return contexts;
    }

    private static List<Dictionary<string, object?>> FinishAll(List<PropertyRequest> requests, bool debug)
    {
        var rows = new List<Dictionary<string, object?>>();
        Exception? failure = null;

        // Every request is finished (and so cleaned up) even if one fails.
        foreach (var request in requests)
        {
            try
            {
                rows.Add(request.Finish(debug));
            }
            catch (Exception exception)
            {
                failure ??= exception;
            }
        }

        if (failure is not null)
        {
            throw failure;
        }

        return rows;
    }
}
