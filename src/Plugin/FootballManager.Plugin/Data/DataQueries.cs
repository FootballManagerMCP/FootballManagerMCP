using SI.Core;

namespace FootballManager.Plugin.Data;

// Runs a DataQuery through PropertyRequests: the base record and the list to fan out over are read and copied first,
// then the requested paths are read on each target. Nothing needs a screen open: roots like "Human" (the user's
// manager) and "game" are always there.
internal static class DataQueries
{
    public const string HumanRoot = "Human";
    public const string SquadPath = "Team.Players";

    public static readonly string[] DefaultPlayerProperties =
    [
        "Name", "Age", "UniqueId", "PositionCombinedString", "Nation.Name", "Condition", "Sharpness", "IsInjured",
        "MoraleString", "BestRole", "AverageRating"
    ];

    public static async Task<string> RunAsync(DataQuery query, TimeSpan timeout, bool debug)
    {
        var pathContexts = await ResolveContextPathsAsync(query, timeout);
        var (record, items) = await ResolveAsync(query, pathContexts, timeout);

        if (!query.FansOut)
        {
            var result = await MainThreadDispatcher.RunAcrossFramesAsync(
                () => PropertyRequest.Start(record ?? RootValue(query.Root), query.Paths, Contexts(query, pathContexts)),
                request => request.IsComplete,
                request => request.Finish(debug),
                timeout);

            return Json.Serialize(result);
        }

        var rows = await MainThreadDispatcher.RunAcrossFramesAsync(
            () => StartTargets(query, pathContexts, record, items),
            batch => batch.Requests.All(r => r.IsComplete),
            batch => FinishTargets(batch, debug),
            timeout);

        return Json.Serialize(new Dictionary<string, object?> { ["count"] = rows.Count, ["items"] = rows });
    }

    private static async Task<List<RequestContext>> ResolveContextPathsAsync(DataQuery query, TimeSpan timeout)
    {
        if (query.ContextPaths is not { Count: > 0 } contextPaths)
        {
            return [];
        }

        var captures = contextPaths.Select(c => c.Path).Distinct().ToDictionary(path => path, _ => PropertyCapture.Value);

        var bindings = await MainThreadDispatcher.RunAcrossFramesAsync(
            () => PropertyRequest.Start(RootValue(query.Root), captures.Keys.ToList(), Contexts(query, []), captures),
            request => request.IsComplete,
            request => request.FinishWithCaptures(),
            timeout);

        return contextPaths
            .Select(c => new RequestContext(c.Name, Captured(bindings, c.Path, "a record").CapturedValue!))
            .ToList();
    }

    private static async Task<(TypedValue? Record, List<TypedValue>? Items)> ResolveAsync(
        DataQuery query, IReadOnlyList<RequestContext> pathContexts, TimeSpan timeout)
    {
        var captures = new Dictionary<string, PropertyCapture>();

        if (query.BasePath is not null)
        {
            captures[query.BasePath] = PropertyCapture.Value;
        }

        if (query.ListPath is not null)
        {
            captures[query.ListPath] = PropertyCapture.Items;
        }

        if (captures.Count == 0)
        {
            return (null, null);
        }

        var bindings = await MainThreadDispatcher.RunAcrossFramesAsync(
            () => PropertyRequest.Start(RootValue(query.Root), captures.Keys.ToList(), Contexts(query, pathContexts), captures),
            request => request.IsComplete,
            request => request.FinishWithCaptures(),
            timeout);

        var record = query.BasePath is null ? null : Captured(bindings, query.BasePath, "a record").CapturedValue;
        var items = query.ListPath is null ? null : Captured(bindings, query.ListPath, "a list").CapturedItems;

        if (items is not null && query.Index is not null)
        {
            if (query.Index < 0 || query.Index >= items.Count)
            {
                throw new ArgumentException($"'{query.ListPath}' has {items.Count} items; index must be 0 to {items.Count - 1}.");
            }

            items = [items[query.Index.Value]];
        }

        return (record, items);
    }

    private static PropertyBinding Captured(IReadOnlyList<PropertyBinding> bindings, string path, string expected)
    {
        var binding = bindings.First(b => b.Path == path);

        if (binding.CapturedValue is null && binding.CapturedItems is null)
        {
            throw new InvalidOperationException(binding.Error ?? $"The game didn't send '{path}' in time (or it isn't {expected}).");
        }

        return binding;
    }

    // One request per list item and/or range value; each row is labelled with the context values it ran with.
    private static (List<PropertyRequest> Requests, List<Dictionary<string, object?>> Labels) StartTargets(
        DataQuery query, IReadOnlyList<RequestContext> pathContexts, TypedValue? record, List<TypedValue>? items)
    {
        var shared = Contexts(query, pathContexts);
        var source = record ?? RootValue(query.Root) ?? throw new InvalidOperationException("The source record has no value yet.");
        IReadOnlyList<TypedValue?> itemsOrNone = items is null ? new TypedValue?[] { null } : items;
        IReadOnlyList<int?> valuesOrNone = query.Each is null ? [null] : query.Each.Values().Select(v => (int?)v).ToList();

        var targets = new List<(TypedValue Source, IReadOnlyList<RequestContext> Contexts)>();
        var labels = new List<Dictionary<string, object?>>();

        foreach (var item in itemsOrNone)
        {
            foreach (var value in valuesOrNone)
            {
                var target = source;
                var contexts = new List<RequestContext>(shared);
                var label = new Dictionary<string, object?>();

                if (item is not null && query.ItemAs is null)
                {
                    target = item;
                }
                else if (item is not null)
                {
                    contexts.Add(new RequestContext(query.ItemAs!, item));
                    label[query.ItemAs!] = TypedValueReader.Read(item);
                }

                if (value is not null)
                {
                    contexts.Add(new ContextLiteral(query.Each!.Name, value.Value).ToRequestContext());
                    label[query.Each.Name] = value;
                }

                targets.Add((target, contexts));
                labels.Add(label);
            }
        }

        return (PropertyRequest.StartAll(targets, query.Paths), labels);
    }

    private static List<Dictionary<string, object?>> FinishTargets(
        (List<PropertyRequest> Requests, List<Dictionary<string, object?>> Labels) batch, bool debug)
    {
        var rows = new List<Dictionary<string, object?>>();
        Exception? failure = null;

        // Every request is finished (and so cleaned up) even if one fails.
        for (var i = 0; i < batch.Requests.Count; i++)
        {
            try
            {
                var row = batch.Requests[i].Finish(debug);

                if (batch.Labels[i].Count > 0)
                {
                    row["context"] = batch.Labels[i];
                }

                rows.Add(row);
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

    private static TypedValue? RootValue(string root)
    {
        return GameBindings.ValueOf(GameBindings.FindRoot(root));
    }

    // The query's literal and path contexts, plus the user's manager and team as "human" and "team" (what the game's
    // own screens pass) when asked for.
    private static List<RequestContext> Contexts(DataQuery query, IReadOnlyList<RequestContext> pathContexts)
    {
        var contexts = (query.Contexts ?? []).Select(c => c.ToRequestContext()).Concat(pathContexts).ToList();

        if (!query.WithHumanAndTeam)
        {
            return contexts;
        }

        var human = GameBindings.FindRoot(HumanRoot);
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
}
