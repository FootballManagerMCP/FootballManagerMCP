using System.Globalization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Clients;

// A /get query for the plugin: the record to read from (a root, optionally a base path under it), what to fan out over
// (a list's items and/or a range of context values), the contexts to pass, and the property paths to read.
internal sealed class BridgeQuery
{
    private readonly List<(string Name, string Value)> _parameters = [];
    private readonly List<string> _paths = [];

    private BridgeQuery(string root)
    {
        Add("root", root);
    }

    public static BridgeQuery FromHuman()
    {
        return new BridgeQuery(GameConstants.HumanRoot);
    }

    public BridgeQuery Base(string path) => Add("base", path);

    public BridgeQuery List(string path) => Add("list", path);

    public BridgeQuery Index(int index) => Add("index", Format(index));

    /// <summary>Passes each list item as the named context instead of reading from it.</summary>
    public BridgeQuery ItemAs(string context) => Add("itemAs", context);

    /// <summary>Reads once per value of the named context, from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public BridgeQuery Each(string context, int from, int to) => Add("each", $"{context}:{Format(from)}:{Format(to)}");

    public BridgeQuery Context(string name, bool value) => Add("ctx", $"{name}:{(value ? "true" : "false")}");

    public BridgeQuery Context(string name, long value) => Add("ctx", $"{name}:{Format(value)}");

    // Masks use all 64 bits; the plugin takes them as signed and hands the game the same bits.
    public BridgeQuery Context(string name, ulong value) => Context(name, unchecked((long)value));

    /// <summary>Passes the record at <paramref name="path"/> under the root as the named context.</summary>
    public BridgeQuery Ref(string name, string path) => Add("ref", $"{name}:{path}");

    /// <summary>Passes the user's manager and team as "human" and "team", as the game's own screens do.</summary>
    public BridgeQuery WithHumanAndTeam() => Add("contexts", "true");

    public BridgeQuery Props(IEnumerable<string> paths)
    {
        _paths.AddRange(paths);
        return this;
    }

    public override string ToString()
    {
        var parameters = _parameters
            .Append((Name: "props", Value: string.Join(',', _paths)))
            .Select(p => $"{p.Name}={Uri.EscapeDataString(p.Value)}");

        return $"/get?{string.Join('&', parameters)}";
    }

    private BridgeQuery Add(string name, string value)
    {
        _parameters.Add((name, value));
        return this;
    }

    private static string Format(long value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}
