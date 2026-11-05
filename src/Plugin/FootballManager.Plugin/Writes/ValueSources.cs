using System.Globalization;
using FootballManager.Plugin.Data;
using FootballManager.Plugin.Inspection;
using SI.Core;

namespace FootballManager.Plugin.Writes;

// Turns value sources into the game's own values, read from one record with the given contexts. Everything returned
// is a copy, so it stays valid however long the write takes.
internal static class ValueSources
{
    public static async Task<Dictionary<string, TypedValue>> ResolveAsync(
        TypedValue source, IReadOnlyList<RequestContext> contexts, IReadOnlyDictionary<string, ValueSource> sources, TimeSpan timeout)
    {
        var captures = new Dictionary<string, PropertyCapture>();

        foreach (var value in sources.Values.Where(v => v.Path is not null))
        {
            captures[value.Path!] = value.IsListItem ? PropertyCapture.Tree : PropertyCapture.Value;
        }

        IReadOnlyList<PropertyBinding> bindings = [];

        if (captures.Count > 0)
        {
            bindings = await MainThreadDispatcher.RunAcrossFramesAsync(
                () => PropertyRequest.Start(source, captures.Keys.ToList(), contexts, captures),
                request => request.IsComplete,
                request => request.FinishWithCaptures(),
                timeout);
        }

        return await MainThreadDispatcher.RunAsync(() => sources.ToDictionary(
            entry => entry.Key,
            entry => Resolve(entry.Key, entry.Value, bindings)));
    }

    private static TypedValue Resolve(string name, ValueSource value, IReadOnlyList<PropertyBinding> bindings)
    {
        if (value.Literal is not null)
        {
            return value.Literal.ToRequestContext().Value;
        }

        var binding = bindings.First(b => b.Path == value.Path);

        if (binding.Error is not null)
        {
            throw new ArgumentException($"{name}: {binding.Error}");
        }

        if (!value.IsListItem)
        {
            return binding.CapturedValue ?? throw new ArgumentException($"{name}: the game didn't answer '{value.Path}'.");
        }

        var rows = binding.CapturedTree ?? throw new ArgumentException($"{name}: '{value.Path}' isn't a list (or wasn't answered).");
        var seen = new List<string>();

        // Rows come top level first, so a match among the top rows wins over one nested in a tree.
        foreach (var (row, decoded) in rows)
        {
            var found = FieldValues(decoded, value.MatchField!).ToList();
            seen.AddRange(found);

            if (found.Contains(value.MatchValue!, StringComparer.OrdinalIgnoreCase))
            {
                return TypedValue.CreateCopyFrom(ItemField(name, row, value.ItemFields));
            }
        }

        throw new ArgumentException(
            $"{name}: no item of '{value.Path}' has {value.MatchField}={value.MatchValue}. Its values: {string.Join(", ", seen.Distinct())}.");
    }

    // The value at a dotted path of field names inside a matched item, or the item itself when no path is given.
    private static TypedValue ItemField(string name, TypedValue item, IReadOnlyList<string>? fields)
    {
        var current = item;

        foreach (var field in fields ?? [])
        {
            var record = TypedValueReader.AsRecord(current)
                ?? throw new ArgumentException($"{name}: the item has no records to find '{field}' in.");

            current = TypedValueReader.Field(current, field)
                ?? throw new ArgumentException($"{name}: the item has no field '{field}'. Its fields: {string.Join(", ", FieldNames(record))}.");
        }

        return current;
    }

    private static IEnumerable<string> FieldNames(Record record)
    {
        foreach (var pair in record)
        {
            yield return IdentifierCatalog.NameOf(pair.Key);
        }
    }

    // Every value of the named field in a row's own decoded fields, nested records included (dropdown rows nest it, e.g.
    // RowData.DisplayValue.Role), but not in its lists: those hold the rows below it, which are matched on their own.
    private static IEnumerable<string> FieldValues(object? decoded, string field)
    {
        if (decoded is not Dictionary<string, object?> record)
        {
            yield break;
        }

        foreach (var (key, child) in record)
        {
            if (string.Equals(key, field, StringComparison.OrdinalIgnoreCase) && child is not null and not Dictionary<string, object?> and not List<object?>)
            {
                yield return Convert.ToString(child, CultureInfo.InvariantCulture) ?? "";
            }

            foreach (var nested in FieldValues(child, field))
            {
                yield return nested;
            }
        }
    }
}
