using System.Globalization;
using SI.Core;

namespace FootballManager.Plugin.Data;

// Paths to a record or a list that go through a list's item: "Team.Division.TeamList[UniqueId=1556].FixtureList" reads
// the league's teams, picks the one whose UniqueId is 1556, and reads that team's fixtures. A selector is an index
// ([3]) or a property and its value ([Name=Hearts], compared as text, ignoring case). The game's bindings know no such
// paths, so each part before a selector is read as a list, the item picked, and the rest read from that item.
internal static class RecordPaths
{
    public static bool HasSelector(string? path)
    {
        return path?.Contains('[') == true;
    }

    /// <summary>
    /// Resolves every selector of <paramref name="path"/> from <paramref name="root"/>: the record the rest of the path
    /// is read from, and that rest (empty when the path ends with a selector, the record itself being the target).
    /// </summary>
    public static async Task<(TypedValue Source, string Remainder)> WalkAsync(
        TypedValue root, string path, IReadOnlyList<RequestContext> contexts, TimeSpan timeout)
    {
        var source = root;
        var rest = path;

        while (rest.IndexOf('[') is var open and >= 0)
        {
            var close = rest.IndexOf(']', open);

            if (open == 0 || close < 0)
            {
                throw new ArgumentException($"'{path}': a selector follows a list's path and ends with ']', e.g. TeamList[UniqueId=1556].");
            }

            var after = rest[(close + 1)..];

            if (after.Length > 0 && after[0] != '.')
            {
                throw new ArgumentException($"'{path}': after a selector comes '.' and the rest of the path.");
            }

            var listPath = rest[..open];
            var items = await ItemsAsync(source, listPath, contexts, timeout);

            source = await SelectAsync(items, rest[(open + 1)..close], listPath, contexts, timeout);
            rest = after.TrimStart('.');
        }

        return (source, rest);
    }

    private static async Task<List<TypedValue>> ItemsAsync(
        TypedValue source, string path, IReadOnlyList<RequestContext> contexts, TimeSpan timeout)
    {
        var bindings = await MainThreadDispatcher.RunAcrossFramesAsync(
            () => PropertyRequest.Start(source, [path], contexts, new Dictionary<string, PropertyCapture> { [path] = PropertyCapture.Items }),
            request => request.IsComplete,
            request => request.FinishWithCaptures(),
            timeout);

        return bindings[0].CapturedItems
            ?? throw new InvalidOperationException(bindings[0].Error ?? $"The game didn't send '{path}' in time (or it isn't a list).");
    }

    private static async Task<TypedValue> SelectAsync(
        List<TypedValue> items, string selector, string listPath, IReadOnlyList<RequestContext> contexts, TimeSpan timeout)
    {
        if (int.TryParse(selector, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
        {
            return index >= 0 && index < items.Count
                ? items[index]
                : throw new ArgumentException($"'{listPath}' has {items.Count} items; [{selector}] must be 0 to {items.Count - 1}.");
        }

        var equals = selector.IndexOf('=');

        if (equals <= 0)
        {
            throw new ArgumentException($"A selector is an index or Property=value, got [{selector}].");
        }

        var property = selector[..equals];
        var wanted = selector[(equals + 1)..];

        if (items.Count == 0)
        {
            throw new ArgumentException($"'{listPath}' is empty: nothing has {property} '{wanted}'.");
        }

        // The property of every item at once, then the first whose value reads as the one wanted.
        var values = await MainThreadDispatcher.RunAcrossFramesAsync(
            () => PropertyRequest.StartAll(items.Select(item => (item, contexts)), [property]),
            requests => requests.All(request => request.IsComplete),
            requests => requests.Select(request => request.Finish(debug: false)).ToList(),
            timeout);

        for (var i = 0; i < items.Count; i++)
        {
            if (values[i]["values"] is Dictionary<string, object?> read
                && read.TryGetValue(property, out var value)
                && string.Equals(Text(value), wanted, StringComparison.OrdinalIgnoreCase))
            {
                return items[i];
            }
        }

        throw new ArgumentException($"No item of '{listPath}' has {property} '{wanted}'.");
    }

    private static string? Text(object? value)
    {
        return value as string ?? Convert.ToString(value, CultureInfo.InvariantCulture);
    }
}
