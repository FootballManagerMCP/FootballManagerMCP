using FootballManager.Domain.Scouting;
using FootballManager.Infrastructure.Dtos.Scouting;

namespace FootballManager.Infrastructure.Mappings;

// The scouting area dropdown as a flat list. Its headings are in the game's language, so an area's kind comes from its
// record's type (the top byte of its database reference); its group is the heading just above it (a continent for a
// nation, a country for a competition). The dropdown also lists recent picks again under a heading of their own.
internal static class ScoutingAreaTree
{
    private const int TypeShift = 24;

    private static readonly Dictionary<long, ScoutingAreaKind> Kinds = new()
    {
        [5] = ScoutingAreaKind.Continent,
        [10] = ScoutingAreaKind.Nation,
        [19] = ScoutingAreaKind.Region,
        [25] = ScoutingAreaKind.Competition
    };

    public static List<ScoutingArea> Flatten(List<BridgeAreaOptionDto> rows)
    {
        var areas = new Dictionary<long, ScoutingArea>();
        Walk(rows, group: null, depth: 0, areas);
        return areas.Values.ToList();
    }

    private static void Walk(List<BridgeAreaOptionDto> rows, string? group, int depth, Dictionary<long, ScoutingArea> areas)
    {
        foreach (var row in rows)
        {
            var heading = row.RowData?.DisplayValue;

            if (row.ChildList is { Count: > 0 } children)
            {
                // The top headings are kinds ("Nation"); the ones below them name the group.
                Walk(children, depth == 0 ? null : heading, depth + 1, areas);
                continue;
            }

            if (row.PropertyValue?.DatabaseReferenceField is not { } id || string.IsNullOrWhiteSpace(row.DisplayValue))
            {
                continue;
            }

            // A recent pick repeats an area listed in its own place, where it has its group.
            if (Kinds.TryGetValue(id >> TypeShift, out var kind) && (!areas.TryGetValue(id, out var known) || known.Group is null))
            {
                areas[id] = new ScoutingArea(id, row.DisplayValue, kind, group);
            }
        }
    }
}
