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

    // The one record a write acts on (the query's base, or its root) with the contexts to pass, copied so it stays
    // valid across the frames the write takes.
    public static async Task<(TypedValue Source, List<RequestContext> Contexts)> ResolveSourceAsync(DataQuery query, TimeSpan timeout)
    {
        var pathContexts = await ResolveContextPathsAsync(query, timeout);
        var (record, _) = await ResolveAsync(query, pathContexts, timeout);

        return await MainThreadDispatcher.RunAsync(() =>
        {
            var source = record ?? RootValue(query.Root) ?? throw new InvalidOperationException("The source record has no value yet.");
            return (TypedValue.CreateCopyFrom(source), Contexts(query, pathContexts));
        });
    }

    private static async Task<List<RequestContext>> ResolveContextPathsAsync(DataQuery query, TimeSpan timeout)
    {
        if (query.ContextPaths is not { Count: > 0 } contextPaths)
        {
            return [];
        }

        // A path through a list item (RecordPaths) is resolved on its own; the plain ones together.
        var contexts = new Dictionary<string, TypedValue>();
        var selected = contextPaths.Select(c => c.Path).Where(RecordPaths.HasSelector).Distinct().ToList();

        if (selected.Count > 0)
        {
            var literals = await MainThreadDispatcher.RunAsync(() => Contexts(query, []));

            foreach (var path in selected)
            {
                contexts[path] = (await ReadAsync(query, literals, path, PropertyCapture.Value, timeout)).Value!;
            }
        }

        var plain = contextPaths.Select(c => c.Path).Where(path => !RecordPaths.HasSelector(path)).Distinct().ToList();

        if (plain.Count > 0)
        {
            var captures = plain.ToDictionary(path => path, _ => PropertyCapture.Value);

            var bindings = await MainThreadDispatcher.RunAcrossFramesAsync(
                () => PropertyRequest.Start(RootValue(query.Root), plain, Contexts(query, []), captures),
                request => request.IsComplete,
                request => request.FinishWithCaptures(),
                timeout);

            foreach (var path in plain)
            {
                contexts[path] = Captured(bindings, path, "a record").CapturedValue!;
            }
        }

        return contextPaths.Select(c => new RequestContext(c.Name, contexts[c.Path])).ToList();
    }

    // A record (Value) or a list (Items) at a path through list items, from the query's root. The contexts come from
    // the main thread, like every game value.
    private static async Task<(TypedValue? Value, List<TypedValue>? Items)> ReadAsync(
        DataQuery query, IReadOnlyList<RequestContext> contexts, string path, PropertyCapture capture, TimeSpan timeout)
    {
        var root = await MainThreadDispatcher.RunAsync(() => RootValue(query.Root) is { } value
            ? TypedValue.CreateCopyFrom(value)
            : throw new InvalidOperationException("The source record has no value yet."));
        var (source, rest) = await RecordPaths.WalkAsync(root, path, contexts, timeout);

        if (rest.Length == 0)
        {
            return capture == PropertyCapture.Value
                ? (source, null)
                : throw new ArgumentException($"'{path}' ends with a selector, which picks a record, not a list.");
        }

        var bindings = await MainThreadDispatcher.RunAcrossFramesAsync(
            () => PropertyRequest.Start(source, [rest], contexts, new Dictionary<string, PropertyCapture> { [rest] = capture }),
            request => request.IsComplete,
            request => request.FinishWithCaptures(),
            timeout);

        var binding = Captured(bindings, rest, capture == PropertyCapture.Value ? "a record" : "a list");
        return (binding.CapturedValue, binding.CapturedItems);
    }

    private static async Task<(TypedValue? Record, List<TypedValue>? Items)> ResolveAsync(
        DataQuery query, IReadOnlyList<RequestContext> pathContexts, TimeSpan timeout)
    {
        if (RecordPaths.HasSelector(query.BasePath) || RecordPaths.HasSelector(query.ListPath))
        {
            return await ResolveThroughItemsAsync(query, pathContexts, timeout);
        }

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

        return (record, Indexed(query, items));
    }

    // The base and list one after the other, either of them going through list items.
    private static async Task<(TypedValue? Record, List<TypedValue>? Items)> ResolveThroughItemsAsync(
        DataQuery query, IReadOnlyList<RequestContext> pathContexts, TimeSpan timeout)
    {
        var contexts = await MainThreadDispatcher.RunAsync(() => Contexts(query, pathContexts));

        var record = query.BasePath is null
            ? null
            : (await ReadAsync(query, contexts, query.BasePath, PropertyCapture.Value, timeout)).Value;
        var items = query.ListPath is null
            ? null
            : (await ReadAsync(query, contexts, query.ListPath, PropertyCapture.Items, timeout)).Items;

        return (record, Indexed(query, items));
    }

    private static List<TypedValue>? Indexed(DataQuery query, List<TypedValue>? items)
    {
        if (items is null || query.Index is null)
        {
            return items;
        }

        if (query.Index < 0 || query.Index >= items.Count)
        {
            throw new ArgumentException($"'{query.ListPath}' has {items.Count} items; index must be 0 to {items.Count - 1}.");
        }

        return [items[query.Index.Value]];
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
