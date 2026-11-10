namespace FootballManager.Plugin.Data;

// What /get reads: a record (a root node, or BasePath under it), optionally run once for each item of ListPath under
// the root and/or each value of a context range, reading Paths on each. A list item is the record read, unless ItemAs
// names the context it's passed as instead (e.g. each player as "Person" into a tactic's role ratings). ContextPaths
// are records read first (off the root, or the root they name) and passed as contexts to everything after. Skip and
// Take read a page of the list (the answer's total is then the whole list's size): a long list, like a player search's
// thousand results, takes half a minute to read in full.
internal sealed record DataQuery(
    string Root,
    IReadOnlyList<string> Paths,
    string? BasePath = null,
    string? ListPath = null,
    string? ItemAs = null,
    int? Index = null,
    ContextRange? Each = null,
    IReadOnlyList<ContextLiteral>? Contexts = null,
    IReadOnlyList<ContextPath>? ContextPaths = null,
    bool WithHumanAndTeam = false,
    int? Skip = null,
    int? Take = null)
{
    public bool FansOut => ListPath is not null || Each is not null;

    public bool Paged => Skip is not null || Take is not null;
}
