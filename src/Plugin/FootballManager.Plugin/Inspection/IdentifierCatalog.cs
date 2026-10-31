using System.Text;
using SI.Bindable.Reference.Core;

namespace FootballManager.Plugin.Inspection;

// The game registers every bindable property, record type and context by id and name at startup.
internal static class IdentifierCatalog
{
    private static readonly Lazy<HashSet<string>> PropertyNames = new(() => Entries(PropertyIdentifierSet.Instance.m_idToInfo)
        .Select(e => e.Name)
        .ToHashSet(StringComparer.OrdinalIgnoreCase));

    public static string Describe(string? set, string? filter)
    {
        var entries = set switch
        {
            "reference" => Entries(ReferenceIdentifierSet.Instance.m_idToInfo),
            "context" => Entries(ContextIdentifierSet.Instance.m_idToInfo),
            _ => Entries(PropertyIdentifierSet.Instance.m_idToInfo)
        };

        var output = new StringBuilder();
        var shown = 0;

        foreach (var (id, name) in entries)
        {
            if (!string.IsNullOrWhiteSpace(filter) && !name.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            output.Append(id).Append('\t').AppendLine(name);
            shown++;
        }

        return $"{shown} of {entries.Count} {set ?? "property"} identifiers\n{output}";
    }

    public static string PropertyName(uint id)
    {
        return PropertyIdentifierSet.Instance.HasID(id) ? PropertyIdentifierSet.Instance.GetInfo(id).Name : $"#{id}";
    }

    public static string ReferenceName(uint id)
    {
        return ReferenceIdentifierSet.Instance.HasID(id) ? ReferenceIdentifierSet.Instance.GetInfo(id).Name : $"#{id}";
    }

    public static string ContextName(uint id)
    {
        return ContextIdentifierSet.Instance.HasID(id) ? ContextIdentifierSet.Instance.GetInfo(id).Name : $"#{id}";
    }

    // Ids inside a reference may come from any of the sets.
    public static string NameOf(uint id)
    {
        return TryNameOf(id) ?? $"#{id}";
    }

    // Values like a team instruction's option are ids into these sets too (e.g. "MuchHigherTempo"); null if unknown.
    public static string? TryNameOf(uint id)
    {
        if (ReferenceIdentifierSet.Instance.HasID(id))
        {
            return ReferenceIdentifierSet.Instance.GetInfo(id).Name;
        }

        if (PropertyIdentifierSet.Instance.HasID(id))
        {
            return PropertyIdentifierSet.Instance.GetInfo(id).Name;
        }

        return ContextIdentifierSet.Instance.HasID(id) ? ContextIdentifierSet.Instance.GetInfo(id).Name : null;
    }

    public static Dictionary<string, string?> NamesOf(string? ids)
    {
        var names = new Dictionary<string, string?>();

        foreach (var text in (ids ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            names[text] = uint.TryParse(text, out var id) ? TryNameOf(id) : null;
        }

        return names;
    }

    public static bool IsProperty(string name)
    {
        return PropertyNames.Value.Contains(name);
    }

    private static List<(uint Id, string Name)> Entries<TInfo>(Il2CppSystem.Collections.Generic.Dictionary<uint, TInfo> map)
        where TInfo : IdentifierInfo
    {
        var entries = new List<(uint Id, string Name)>();

        foreach (var pair in map)
        {
            entries.Add((pair.Key, pair.Value.Name));
        }

        return entries.OrderBy(e => e.Id).ToList();
    }
}
