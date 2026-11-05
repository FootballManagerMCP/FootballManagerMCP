using System.Globalization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Clients;

// A /get query for the plugin: the record to read from (a root, optionally a base path under it), what to fan out over
// (a list's items and/or a range of context values), the contexts to pass, and the property paths to read.
internal sealed class BridgeQuery
{
    // A write reads the value before and after, a few frames each.
    private const string WriteTimeoutMs = "8000";

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

    /// <summary>
    /// The tactic the team plays. Reads pass the user's manager and team as contexts, like the game's screens; writes
    /// don't, like the tactics screen's controls.
    /// </summary>
    public static BridgeQuery OnActiveTactic(bool forWrite = false)
    {
        var query = FromHuman().Base(TacticConstants.Tactic).Context(TacticConstants.TacticFlag, true);
        return forWrite ? query : query.WithHumanAndTeam();
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
        return Url("/get", _parameters.Append((Name: "props", Value: string.Join(',', _paths))));
    }

    /// <summary>
    /// A write to the record this query reads, with its contexts: the plugin's /set (path, value) or /native (event,
    /// field). Writes are confirmed here: the caller has the user's agreement by the time it writes.
    /// </summary>
    public string ToWrite(string endpoint, params (string Name, string Value)[] parameters)
    {
        return Write(endpoint, [.. _parameters, .. parameters]);
    }

    /// <summary>A write that takes no record, e.g. the plugin's /move or /pick.</summary>
    public static string Write(string endpoint, IEnumerable<(string Name, string Value)> parameters)
    {
        return Url(endpoint, parameters.Append(("confirm", "true")).Append(("timeoutMs", WriteTimeoutMs)));
    }

    private static string Url(string endpoint, IEnumerable<(string Name, string Value)> parameters)
    {
        return $"{endpoint}?{string.Join('&', parameters.Select(p => $"{p.Name}={Uri.EscapeDataString(p.Value)}"))}";
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
