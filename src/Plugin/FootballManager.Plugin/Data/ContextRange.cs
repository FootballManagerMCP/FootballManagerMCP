namespace FootballManager.Plugin.Data;

// A context to run the request once for each whole number of, given as Name:from:to, e.g. TeamSelectionIndex:0:10.
internal sealed record ContextRange(string Name, int From, int To)
{
    private const int MaxValues = 100;

    public static ContextRange Parse(string text)
    {
        var parts = text.Split(':');

        if (parts.Length != 3 || !int.TryParse(parts[1], out var from) || !int.TryParse(parts[2], out var to) || to < from)
        {
            throw new ArgumentException($"A range is Name:from:to, got '{text}'.");
        }

        if (to - from + 1 > MaxValues)
        {
            throw new ArgumentException($"A range covers at most {MaxValues} values.");
        }

        return new ContextRange(parts[0], from, to);
    }

    public IEnumerable<int> Values()
    {
        return Enumerable.Range(From, To - From + 1);
    }
}
