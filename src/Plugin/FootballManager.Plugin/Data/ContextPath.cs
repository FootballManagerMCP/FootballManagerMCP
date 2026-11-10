namespace FootballManager.Plugin.Data;

// A context given in a query as Name:path, passing the record at that path under the root (e.g. HumanTeam:Team, the
// user's team, which the tactical effectiveness screens ask for under that name). The path can start from another
// root, as Name:Root/path (Records:Human/ScoutingCentreActions.Players[UniqueId=24057535], for a query on "game").
// As a list (refList=), the record goes as the one item of a list, as contexts like "Records" (the records a menu
// action applies to) take it.
internal sealed record ContextPath(string Name, string Path, string? Root = null, bool AsList = false)
{
    // Where the record is read, whatever the context's name and form: two contexts on one record read it once.
    public string Key => $"{Root}/{Path}";

    public static ContextPath Parse(string text)
    {
        return Parse(text, asList: false);
    }

    public static ContextPath ParseList(string text)
    {
        return Parse(text, asList: true);
    }

    private static ContextPath Parse(string text, bool asList)
    {
        var separator = text.IndexOf(':');

        if (separator <= 0 || separator == text.Length - 1)
        {
            throw new ArgumentException($"A context path is Name:path or Name:Root/path, got '{text}'.");
        }

        var name = text[..separator];
        var path = text[(separator + 1)..];

        // A root comes before the path's first property: a '/' after a '.' or '[' is part of the path (a selector's value).
        var slash = path.IndexOf('/');
        var start = path.IndexOfAny(['.', '[']);

        if (slash > 0 && (start < 0 || slash < start))
        {
            return slash == path.Length - 1
                ? throw new ArgumentException($"'{text}' names a root but no path after it.")
                : new ContextPath(name, path[(slash + 1)..], path[..slash], asList);
        }

        return new ContextPath(name, path, AsList: asList);
    }
}
