namespace FootballManager.Plugin.Data;

// A context given in a query as Name:path, passing the record at that path under the root (e.g. HumanTeam:Team, the
// user's team, which the tactical effectiveness screens ask for under that name).
internal sealed record ContextPath(string Name, string Path)
{
    public static ContextPath Parse(string text)
    {
        var separator = text.IndexOf(':');

        if (separator <= 0 || separator == text.Length - 1)
        {
            throw new ArgumentException($"A context path is Name:path, got '{text}'.");
        }

        return new ContextPath(text[..separator], text[(separator + 1)..]);
    }
}
