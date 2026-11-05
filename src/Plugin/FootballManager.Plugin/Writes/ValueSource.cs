using FootballManager.Plugin.Data;

namespace FootballManager.Plugin.Writes;

// Where a value to write comes from, as a query gives it:
//   5, true, 'text', u:2048   a literal (typed like a context literal)
//   @Path                     the value the game holds at that path, e.g. @PlayerFromTeamSelectionIndex
//   @ListPath[Field=value]    the item of that list whose decoded fields include Field=value, e.g. an option of a
//                             dropdown: @MentalityDropdownOptions[PropertyValue=4]
//   @ListPath[Field=value].A  a field inside that item (a dotted path into its records), e.g. the row a role pick sends:
//                             @AvailableRolesDropdown[Role=2048].RowData
internal sealed record ValueSource(
    ContextLiteral? Literal = null, string? Path = null, string? MatchField = null, string? MatchValue = null,
    IReadOnlyList<string>? ItemFields = null)
{
    public bool IsListItem => MatchField is not null;

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

        if (close < 0 || equals < 0 || open == 0 || (rest.Length > 0 && !rest.StartsWith('.')))
        {
            throw new ArgumentException($"A list item is @ListPath[Field=value] or @ListPath[Field=value].Field, got '{text}'.");
        }

        var fields = rest.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

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
        return IsListItem ? $"@{Path}[{MatchField}={MatchValue}]{item}" : $"@{Path}";
    }
}
