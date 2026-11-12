using FootballManager.Infrastructure.Dtos;

namespace FootballManager.Infrastructure.Mappings;

// A training dropdown's choices: its rows (wrapped in RowData after a separator) and the rows of its submenus. A row that
// opens a submenu (the rest periods under "no work") isn't a choice of its own; its rows carry its label too, since two
// submenus can share theirs ("1 semaine" of rest, of holiday). Submenus can be left out by their row's id.
internal static class TrainingRows
{
    public static IEnumerable<BridgeOptionDto> Of(IEnumerable<BridgeOptionDto>? rows, params long[] leftOut)
    {
        foreach (var entry in rows ?? [])
        {
            var row = entry.RowData ?? entry;

            if (entry.ChildList is { Count: > 0 } children)
            {
                if (row.PropertyValue is { } submenu && leftOut.Contains(submenu))
                {
                    continue;
                }

                foreach (var child in children.Select(child => child.RowData ?? child).Where(child => child.PropertyValue is not null))
                {
                    yield return new BridgeOptionDto { PropertyValue = child.PropertyValue, DisplayValue = $"{row.DisplayValue} ({child.DisplayValue})" };
                }

                continue;
            }

            if (row.PropertyValue is not null)
            {
                yield return row;
            }
        }
    }
}
