using System.Collections.Specialized;
using System.Net;
using System.Text;
using BepInEx.Logging;
using FootballManager.Plugin.Data;
using FootballManager.Plugin.Inspection;
using FootballManager.Plugin.Writes;

namespace FootballManager.Plugin;

internal sealed class BridgeServer(int port, ManualLogSource log) : IDisposable
{
    // Data requests wait a few frames per step for the game to answer; the squad takes two steps.
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(75);
    private static readonly TimeSpan DefaultAnswerTimeout = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan MaxAnswerTimeout = TimeSpan.FromSeconds(30);

    private readonly HttpListener _listener = new();

    public void Start()
    {
        _listener.Prefixes.Add($"http://localhost:{port}/");
        _listener.Start();
        _ = Task.Run(AcceptLoopAsync);

        log.LogInfo($"Listening on http://localhost:{port}/");
    }

    public void Dispose()
    {
        _listener.Close();
    }

    private async Task AcceptLoopAsync()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext context;

            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (Exception) when (!_listener.IsListening)
            {
                return;
            }

            _ = HandleAsync(context);
        }
    }

    private async Task HandleAsync(HttpListenerContext context)
    {
        Reply reply;

        try
        {
            var route = Route(context.Request.Url!.AbsolutePath, context.Request.QueryString);

            if (route is null)
            {
                reply = Reply.Text(404, "Unknown endpoint. Try /status, /ui, /find?text=, /table, /catalog?set=property|reference|context, "
                    + "/bindings?path=&depth=, /schema?type=person|team|club, /names?ids=, /instructions, /player-instructions, "
                    + "/enums?types=FM.UI.InPossessionRole,..., /squad?props=, "
                    + "/player?index=&props=, or /get?root=Human&props= with optional base=, list=, itemAs=, index=, "
                    + "each=Name:from:to, ctx=Name:value and ref=Name:path (both repeatable), contexts=true, debug=true, timeoutMs=. "
                    + "Writes (confirm=true): /set?<as /get>&path=&value=5|u:5|@Path|@List[Field=value]|@List[Field=value].Field, "
                    + "/event?<as /get>&type=role|formation|selection|modify&field=Name:<value> (repeatable)&template=none&dryRun=true, "
                    + "/native?<as /get>&event=EventFmx...&field=Target:<value>&field=Data:<value>|data=<json record>&template=none"
                    + "&dryRun=true, "
                    + "/move?index=<0-10>&to=<position mask>|with=<0-10>&inPossession=true|false, "
                    + "/pick?index=<0-10>&player=<database index> (dryRun=true to only build either). "
                    + "Recording: /events?start=true[&interop=false][&writes=false][&ui=false][&data=true]|stop=true|clear=true, "
                    + "read with /events[?stream=data].");
            }
            else
            {
                var task = route();

                reply = await Task.WhenAny(task, Task.Delay(RequestTimeout)) == task
                    ? await task
                    : Reply.Text(504, "The game's main thread didn't answer in time (is the game busy processing?).");
            }
        }
        catch (ArgumentException exception)
        {
            reply = Reply.Text(400, exception.Message);
        }
        catch (Exception exception)
        {
            reply = Reply.Text(500, exception.ToString());
            log.LogError(exception);
        }

        var bytes = Encoding.UTF8.GetBytes(reply.Body);
        context.Response.StatusCode = reply.Status;
        context.Response.ContentType = reply.ContentType;
        context.Response.ContentLength64 = bytes.Length;
        await context.Response.OutputStream.WriteAsync(bytes);
        context.Response.Close();
    }

    private static Func<Task<Reply>>? Route(string path, NameValueCollection query) => path switch
    {
        "/status" => Text(GameStatus.Describe),
        "/ui" => Text(() => UiTreeDumper.Dump(query["visibleOnly"] != "false", ParseInt(query["maxDepth"], 500))),
        "/find" => Text(() => UiTreeDumper.Find(query["text"] ?? "")),
        "/table" => Text(() => TableReader.Read(query["path"])),
        "/catalog" => Text(() => IdentifierCatalog.Describe(query["set"], query["filter"])),
        "/bindings" => Text(() => BindingTreeDumper.Dump(query["path"], ParseInt(query["depth"], 1))),
        "/schema" => Json(() => MainThreadDispatcher.RunAsync(
            () => PropertySchema.Describe(query["type"] ?? "person", query["filter"]))),
        "/names" => Text(() => Data.Json.Serialize(IdentifierCatalog.NamesOf(query["ids"])), "application/json; charset=utf-8"),
        "/instructions" => Text(() => Data.Json.Serialize(TeamInstructionCatalog.Describe()), "application/json; charset=utf-8"),
        "/player-instructions" => Text(() => Data.Json.Serialize(PlayerInstructionCatalog.Describe()), "application/json; charset=utf-8"),
        "/enums" => Text(() => Data.Json.Serialize(EnumCatalog.Describe(query["types"])), "application/json; charset=utf-8"),
        "/get" => Json(() => DataQueries.RunAsync(GetQuery(query), AnswerTimeout(query), query["debug"] == "true")),
        "/set" => Json(() => SetAsync(query)),
        "/event" => Json(() => SendEventAsync(query)),
        "/native" => Json(() => SendNativeEventAsync(query)),
        "/move" => Json(() => MoveAsync(query)),
        "/pick" => Json(() => PickAsync(query)),
        "/events" => Text(() => Events(query), "application/json; charset=utf-8"),
        "/squad" => Json(() => DataQueries.RunAsync(SquadQuery(query, null), AnswerTimeout(query), query["debug"] == "true")),
        "/player" => Json(() => DataQueries.RunAsync(
            SquadQuery(query, ParseInt(query["index"], 0)), AnswerTimeout(query), query["debug"] == "true")),
        _ => null
    };

    private static DataQuery GetQuery(NameValueCollection query)
    {
        return new DataQuery(
            Root: query["root"] ?? DataQueries.HumanRoot,
            Paths: Properties(query, []),
            BasePath: NullIfEmpty(query["base"]),
            ListPath: NullIfEmpty(query["list"]),
            ItemAs: NullIfEmpty(query["itemAs"]),
            Index: query["index"] is null ? null : ParseInt(query["index"], 0),
            Each: NullIfEmpty(query["each"]) is { } each ? ContextRange.Parse(each) : null,
            Contexts: (query.GetValues("ctx") ?? []).Select(ContextLiteral.Parse).ToList(),
            ContextPaths: (query.GetValues("ref") ?? []).Select(ContextPath.Parse).ToList(),
            WithHumanAndTeam: query["contexts"] == "true");
    }

    private static async Task<string> SetAsync(NameValueCollection query)
    {
        var path = NullIfEmpty(query["path"]) ?? throw new ArgumentException("Name the property to set: path=MentalityDropdown.");
        var value = NullIfEmpty(query["value"]) ?? throw new ArgumentException("Give the value: value=5, value=@Path or value=@List[Field=value].");
        var timeout = AnswerTimeout(query);

        await WriteGuard.EnsureAllowedAsync(query["confirm"] == "true", timeout);
        return await PropertyWriter.SetAsync(GetQuery(query), path, ValueSource.Parse(path, value), timeout);
    }

    private static async Task<string> SendEventAsync(NameValueCollection query)
    {
        var type = NullIfEmpty(query["type"]) ?? throw new ArgumentException($"Name the event: type={string.Join("|", EventSender.Types)}.");
        var timeout = AnswerTimeout(query);
        var fields = Fields(query);
        var send = query["dryRun"] != "true";

        if (send)
        {
            await WriteGuard.EnsureAllowedAsync(query["confirm"] == "true", timeout);
        }

        return await EventSender.SendAsync(GetQuery(query), type, fields, query["template"] != "none", send, timeout);
    }

    private static async Task<string> SendNativeEventAsync(NameValueCollection query)
    {
        var name = NullIfEmpty(query["event"]) ?? throw new ArgumentException("Name the native event as /events shows it: event=EventFmxChangeFormation.");
        var timeout = AnswerTimeout(query);
        var fields = Fields(query);
        var send = query["dryRun"] != "true";

        if (send)
        {
            await WriteGuard.EnsureAllowedAsync(query["confirm"] == "true", timeout);
        }

        return await NativeEventSender.SendAsync(
            GetQuery(query), name, fields, NullIfEmpty(query["data"]), query["template"] != "none", send, timeout);
    }

    private static async Task<string> MoveAsync(NameValueCollection query)
    {
        var index = ParseOptionalInt(query["index"]) ?? throw new ArgumentException("Name the lineup position to move: index=0-10.");
        var to = NullIfEmpty(query["to"]) is { } mask
            ? uint.TryParse(mask, out var parsed) ? parsed : throw new ArgumentException($"to is a position mask (a number), got '{mask}'.")
            : (uint?)null;
        var inPossession = query["inPossession"] != "false";
        var send = query["dryRun"] != "true";
        var timeout = AnswerTimeout(query);

        if (send)
        {
            await WriteGuard.EnsureAllowedAsync(query["confirm"] == "true", timeout);
        }

        return await LineupMover.MoveAsync(index, to, ParseOptionalInt(query["with"]), inPossession, send, timeout);
    }

    private static async Task<string> PickAsync(NameValueCollection query)
    {
        var index = ParseOptionalInt(query["index"]) ?? throw new ArgumentException("Name the lineup position to fill: index=0-10.");
        var player = ParseOptionalInt(query["player"]) ?? throw new ArgumentException("Name the player by database index: player=102220.");
        var send = query["dryRun"] != "true";
        var timeout = AnswerTimeout(query);

        if (send)
        {
            await WriteGuard.EnsureAllowedAsync(query["confirm"] == "true", timeout);
        }

        return await LineupPicker.PickAsync(index, player, send, timeout);
    }

    private static int? ParseOptionalInt(string? value)
    {
        return NullIfEmpty(value) is { } text
            ? int.TryParse(text, out var parsed) ? parsed : throw new ArgumentException($"Expected a whole number, got '{text}'.")
            : null;
    }

    // Repeatable field=Name:value, each value a literal, @Path or @List[Field=value].
    private static Dictionary<string, ValueSource> Fields(NameValueCollection query)
    {
        var fields = new Dictionary<string, ValueSource>();

        foreach (var field in query.GetValues("field") ?? [])
        {
            var separator = field.IndexOf(':');

            if (separator <= 0)
            {
                throw new ArgumentException($"A field is Name:value, got '{field}'.");
            }

            var name = field[..separator];
            fields[name] = ValueSource.Parse(name, field[(separator + 1)..]);
        }

        return fields;
    }

    private static string Events(NameValueCollection query)
    {
        var options = new RecorderOptions(
            Interop: query["interop"] != "false",
            Writes: query["writes"] != "false",
            Ui: query["ui"] != "false",
            Data: query["data"] == "true");

        var status = query["start"] == "true" ? EventRecorder.Start(options)
            : query["stop"] == "true" ? EventRecorder.Stop()
            : null;

        if (query["clear"] == "true")
        {
            EventRecorder.Clear();
        }

        var dataStream = query["stream"] == "data";

        return Data.Json.Serialize(new Dictionary<string, object?>
        {
            ["recording"] = EventRecorder.IsRecording,
            ["status"] = status,
            ["stream"] = dataStream ? "data" : "events",
            ["dataCount"] = EventRecorder.DataCount,
            ["events"] = dataStream ? EventRecorder.DescribeData() : EventRecorder.Describe()
        });
    }

    // The user's first-team players, one row each.
    private static DataQuery SquadQuery(NameValueCollection query, int? index)
    {
        return new DataQuery(
            Root: DataQueries.HumanRoot,
            Paths: Properties(query, DataQueries.DefaultPlayerProperties),
            ListPath: DataQueries.SquadPath,
            Index: index,
            Contexts: (query.GetValues("ctx") ?? []).Select(ContextLiteral.Parse).ToList(),
            ContextPaths: (query.GetValues("ref") ?? []).Select(ContextPath.Parse).ToList(),
            WithHumanAndTeam: query["contexts"] == "true");
    }

    private static string? NullIfEmpty(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static Func<Task<Reply>> Text(Func<string> work, string contentType = "text/plain; charset=utf-8")
    {
        return async () => new Reply(200, await MainThreadDispatcher.RunAsync(work), contentType);
    }

    private static Func<Task<Reply>> Json(Func<Task<string>> work)
    {
        return async () => Reply.Json(await work());
    }

    private static IReadOnlyList<string> Properties(NameValueCollection query, IReadOnlyList<string> fallback)
    {
        var requested = (query["props"] ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return requested.Length > 0 ? requested : fallback;
    }

    private static TimeSpan AnswerTimeout(NameValueCollection query)
    {
        var milliseconds = ParseInt(query["timeoutMs"], (int)DefaultAnswerTimeout.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(Math.Clamp(milliseconds, 100, (int)MaxAnswerTimeout.TotalMilliseconds));
    }

    private static int ParseInt(string? value, int fallback)
    {
        return int.TryParse(value, out var parsed) ? parsed : fallback;
    }
}
