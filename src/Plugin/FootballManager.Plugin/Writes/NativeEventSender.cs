using FM.GamePlugin;
using FootballManager.Plugin.Data;
using SI.Core;
using SI.Interop;
using DateTime = System.DateTime;

namespace FootballManager.Plugin.Writes;

// Sends a native event straight to the game, the way its screens do for what has no UI event of its own (changing
// formation, the tactic creator's steps): an event id, the record it targets and a value. Target and Data come from
// the request (Data also as a record built from JSON), or else from the last recorded event of that name, so a real
// click's contents can serve as the template.
//
// With a hold, the base record stays bound from before the event until that long after it, as a screen keeps the
// records it shows bound while the user edits them, and the answer has the query's paths under the base before and
// after.
internal static class NativeEventSender
{
    private const string TargetField = "Target";
    private const string DataField = "Data";

    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(50);

    public static async Task<string> SendAsync(
        DataQuery query, string eventText, IReadOnlyDictionary<string, ValueSource> given, string? dataJson, bool useTemplate,
        bool send, TimeSpan? hold, TimeSpan timeout)
    {
        if (hold is null)
        {
            return Json.Serialize(await SendOnceAsync(query, eventText, given, dataJson, useTemplate, send, null, timeout));
        }

        var (_, contexts) = await DataQueries.ResolveSourceAsync(query, timeout);
        var watch = await MainThreadDispatcher.RunAsync(() => DataQueries.StartWatch(query, contexts));

        try
        {
            var deadline = DateTime.UtcNow + timeout;

            while (DateTime.UtcNow < deadline && !await MainThreadDispatcher.RunAsync(() => watch.IsComplete))
            {
                await Task.Delay(PollInterval);
            }

            var before = await MainThreadDispatcher.RunAsync(watch.Snapshot);
            var bound = watch.PathBindings[0].CapturedValue
                        ?? throw new InvalidOperationException($"The game didn't send '{query.BasePath}' in time.");
            var answer = await SendOnceAsync(query, eventText, given, dataJson, useTemplate, send, bound, timeout);

            await Task.Delay(hold.Value);

            answer["held"] = $"{hold.Value.TotalMilliseconds:0} ms";
            answer["before"] = before["values"];
            answer["after"] = (await MainThreadDispatcher.RunAsync(watch.Snapshot))["values"];
            return Json.Serialize(answer);
        }
        finally
        {
            await MainThreadDispatcher.RunTeardownAsync(() =>
            {
                watch.Dispose();
                return true;
            });
        }
    }

    // The target is the bound record when one is given (it is the query's base), else the base read afresh.
    private static async Task<Dictionary<string, object?>> SendOnceAsync(
        DataQuery query, string eventText, IReadOnlyDictionary<string, ValueSource> given, string? dataJson, bool useTemplate,
        bool send, TypedValue? bound, TimeSpan timeout)
    {
        var eventId = ParseEvent(eventText);
        var name = InteropRecorder.EventName(eventId);
        var unknown = given.Keys.Except([TargetField, DataField], StringComparer.Ordinal).ToList();

        if (unknown.Count > 0)
        {
            throw new ArgumentException($"A native event has no {string.Join(", ", unknown)}; its fields are {TargetField} and {DataField}.");
        }

        if (dataJson is not null && given.ContainsKey(DataField))
        {
            throw new ArgumentException("Give Data once: either field=Data: or data=<json>.");
        }

        var dataRecord = dataJson is null ? (System.Text.Json.JsonElement?)null : RecordBuilder.Parse(dataJson);
        var sources = new Dictionary<string, ValueSource>(given);

        foreach (var (text, recordSource) in dataRecord is { } json ? RecordBuilder.Sources(json) : [])
        {
            sources[text] = recordSource;
        }

        var (source, contexts) = await DataQueries.ResolveSourceAsync(query, timeout);
        var values = await ValueSources.ResolveAsync(source, contexts, sources, timeout);

        return await MainThreadDispatcher.RunAsync(() =>
        {
            var template = useTemplate ? EventRecorder.Last(InteropRecorder.NativeType, name) : null;

            // The record the request reads from is the one the event targets, when it has a base (base=Team.Tactic).
            var baseRecord = query.BasePath is null ? null : TypedValueReader.AsReference(bound ?? source);
            var sent = new RecordedEvent(InteropRecorder.NativeType, name);

            InteropReference? Target()
            {
                if (values.TryGetValue(TargetField, out var value))
                {
                    return TypedValueReader.AsReference(value) ?? throw new ArgumentException("The Target given isn't a game record.");
                }

                if (baseRecord is not null)
                {
                    return baseRecord;
                }

                // A template whose event had no target leaves it null, as the game sent it.
                return template is not null
                    ? template.References.GetValueOrDefault(TargetField)
                    : throw new ArgumentException(
                        $"No Target for {name}: read from base=Team.Tactic, give field=Target:@Path, or record one by hand to use as the template.");
            }

            TypedValue? Data()
            {
                if (dataRecord is { } json)
                {
                    // data=null: the event carries nothing, as many of the game's confirm and cancel events do.
                    return json.ValueKind == System.Text.Json.JsonValueKind.Null ? null : RecordBuilder.Build(json, values);
                }

                if (values.TryGetValue(DataField, out var value))
                {
                    return TypedValue.CreateCopyFrom(value);
                }

                if (template is null)
                {
                    throw new ArgumentException($"No Data for {name}: give field=Data:<value>, or record one by hand to use as the template.");
                }

                return template.Values.TryGetValue(DataField, out var recorded) ? TypedValue.CreateCopyFrom(recorded) : null;
            }

            var target = Target();
            var data = Data();

            sent.AddNote("event", name);
            sent.AddNote("eventId", eventId);
            sent.AddReference(TargetField, target);
            sent.AddValue(DataField, data);

            if (send)
            {
                Subsystems.Get<GameInteropSubsystem>().SendEvent(eventId, target, data);
            }

            return new Dictionary<string, object?>
            {
                ["event"] = name,
                ["dryRun"] = !send,
                ["template"] = template?.Utc.ToString("HH:mm:ss.fff"),
                ["sent"] = sent.Description
            };
        });
    }

    // By name as /events shows it (EventFmxChangeFormation) or by number.
    private static uint ParseEvent(string text)
    {
        if (uint.TryParse(text, out var id))
        {
            return id;
        }

        return Enum.TryParse<InteropEventIDs>(text.TrimStart('#'), ignoreCase: true, out var named)
            ? (uint)named
            : throw new ArgumentException($"Unknown native event '{text}'. Use its name as /events shows it, e.g. EventFmxChangeFormation.");
    }
}
