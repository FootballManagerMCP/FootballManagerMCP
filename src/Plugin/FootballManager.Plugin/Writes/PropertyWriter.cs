using FootballManager.Plugin.Data;
using SI.Core;

namespace FootballManager.Plugin.Writes;

// Sets one property the way the game's dropdowns do (e.g. a tactic's mentality to one of its own options), then
// reads it back so the answer says what the game now holds, not what was sent.
internal static class PropertyWriter
{
    // Frames for the game to take the value before the binding it came through goes away.
    private static readonly TimeSpan SettleTime = TimeSpan.FromMilliseconds(500);

    public static async Task<string> SetAsync(DataQuery query, string path, ValueSource value, TimeSpan timeout)
    {
        var (source, contexts) = await DataQueries.ResolveSourceAsync(query, timeout);
        var resolved = await ValueSources.ResolveAsync(source, contexts, new Dictionary<string, ValueSource> { ["value"] = value }, timeout);
        var newValue = resolved["value"];

        var request = await MainThreadDispatcher.RunAcrossFramesAsync(
            () => PropertyRequest.Start(source, [path], contexts),
            pending => pending.IsComplete,
            pending => pending,
            timeout);

        object? before;

        try
        {
            before = await MainThreadDispatcher.RunAsync(() =>
            {
                var binding = request.PathBindings[0];

                if (binding.Error is not null)
                {
                    throw new ArgumentException(binding.Error);
                }

                var current = binding.Value;
                request.Write(binding, TypedValue.CreateCopyFrom(newValue));
                return current;
            });

            await Task.Delay(SettleTime);
        }
        finally
        {
            await MainThreadDispatcher.RunAsync(() =>
            {
                request.Dispose();
                return true;
            });
        }

        var after = await ReadAsync(source, contexts, path, timeout);
        var written = await MainThreadDispatcher.RunAsync(() => TypedValueReader.Read(newValue));

        return Json.Serialize(new Dictionary<string, object?>
        {
            ["path"] = path,
            ["before"] = before,
            ["written"] = written,
            ["after"] = after,
            ["changed"] = Json.Compact(before) != Json.Compact(after)
        });
    }

    private static Task<object?> ReadAsync(TypedValue source, IReadOnlyList<RequestContext> contexts, string path, TimeSpan timeout)
    {
        return MainThreadDispatcher.RunAcrossFramesAsync(
            () => PropertyRequest.Start(source, [path], contexts),
            request => request.IsComplete,
            request =>
            {
                var value = request.PathBindings[0].Value;
                request.Dispose();
                return value;
            },
            timeout);
    }
}
