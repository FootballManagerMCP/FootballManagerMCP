using FootballManager.Plugin.Data;

namespace FootballManager.Plugin.Writes;

// Where a value to write comes from, as a query gives it:
//   5, true, 'text', u:2048   a literal (typed like a context literal)
//   @Path                     the value the game holds at that path, e.g. @PlayerFromTeamSelectionIndex
//   @ListPath[Field=value]    the item of that list whose decoded fields include Field=value, e.g. an option of a
//                             dropdown: @MentalityDropdownOptions[PropertyValue=4]
//   @ListPath[Field=value].A  a field inside that item (a dotted path into its records), e.g. the row a role pick sends:
//                             @AvailableRolesDropdown[Role=2048].RowData
//   @ListPath[2].A.1.B        the list's third item, and in the path into it a number picks a list's item: a menu's
//                             entries, where labels repeat under each branch (@Items[1].ChildList.2.RowData.ActionData)
internal sealed record ValueSource(
    ContextLiteral? Literal = null, string? Path = null, string? MatchField = null, string? MatchValue = null,
    IReadOnlyList<string>? ItemFields = null, int? MatchIndex = null)
{
    public bool IsListItem => MatchField is not null || MatchIndex is not null;

    public static ValueSource Parse(string name, string text)
    {
        if (!text.StartsWith('@'))
        {
            return new ValueSource(Literal: ContextLiteral.Parse($"{name}:{text}"));
        }

        var path = text[1..];
        var open = path.IndexOf('[');

        if (open < 0)
        {
            return path.Length > 0 ? new ValueSource(Path: path) : throw new ArgumentException($"'{text}' names no path.");
        }

        var close = path.IndexOf(']', open);
        var equals = close < 0 ? -1 : path.IndexOf('=', open, close - open);
        var rest = close < 0 ? "" : path[(close + 1)..];
        var index = close < 0 || equals >= 0 ? (int?)null : ParseIndex(path[(open + 1)..close]);

        if (close < 0 || (equals < 0 && index is null) || open == 0 || (rest.Length > 0 && !rest.StartsWith('.')))
        {
            throw new ArgumentException(
                $"A list item is @ListPath[Field=value] or @ListPath[index], optionally followed by .Field, got '{text}'.");
        }

        var fields = rest.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (index is not null)
        {
            return new ValueSource(Path: path[..open], ItemFields: fields.Length > 0 ? fields : null, MatchIndex: index);
        }

        return new ValueSource(
            Path: path[..open],
            MatchField: path[(open + 1)..equals],
            MatchValue: path[(equals + 1)..close],
            ItemFields: fields.Length > 0 ? fields : null);
    }

    public override string ToString()
    {
        if (Literal is not null)
        {
            return $"{Literal.Value}";
        }

        var item = ItemFields is null ? "" : "." + string.Join('.', ItemFields);

        return MatchIndex is { } index ? $"@{Path}[{index}]{item}"
            : IsListItem ? $"@{Path}[{MatchField}={MatchValue}]{item}"
            : $"@{Path}";
    }

    private static int? ParseIndex(string text)
    {
        return int.TryParse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var index)
            ? index
            : null;
    }
}
