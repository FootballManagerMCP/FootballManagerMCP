using System.Collections.Specialized;
using System.Net;
using System.Text;
using BepInEx.Logging;
using FootballManager.Plugin.Data;
using FootballManager.Plugin.Inspection;

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
                    + "/bindings?path=&depth=, /schema?type=person|team|club, /names?ids=, /squad?props=, /player?index=&props=, "
                    + "or /get?root=Human&props= with optional base=, list=, itemAs=, index=, each=Name:from:to, ctx=Name:value "
                    + "and ref=Name:path (both repeatable), contexts=true, debug=true, timeoutMs=.");
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
        "/get" => Json(() => DataQueries.RunAsync(GetQuery(query), AnswerTimeout(query), query["debug"] == "true")),
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
