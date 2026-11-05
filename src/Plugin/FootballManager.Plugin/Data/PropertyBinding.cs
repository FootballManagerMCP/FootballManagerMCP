using SI.Bindable;
using SI.Core;
using DateTime = System.DateTime;

namespace FootballManager.Plugin.Data;

// One requested path within a PropertyRequest. The value is only valid inside the callback (the game pools values),
// so everything needed later is copied there.
internal sealed class PropertyBinding(string path, PropertyCapture capture)
{
    public string Path { get; } = path;

    public Bindings.Key Key { get; set; }

    // The node the value arrived on, which is the one a write must target (not the key the request returned).
    public Bindings.Key ValueKey { get; private set; }

    // Kept referenced for the binding's lifetime: the game holds only the IL2CPP side of the delegate.
    public Bindings.ValueChangedCallback? Callback { get; set; }

    public bool Received { get; private set; }

    public int CallbackCount { get; private set; }

    public DateTime LastCallbackUtc { get; private set; }

    public object? Value { get; private set; }

    // Copies kept per PropertyCapture, still valid as sources once the request is gone.
    public TypedValue? CapturedValue { get; private set; }

    public List<TypedValue>? CapturedItems { get; private set; }

    // Rows top level first, then each level below, decoded when they arrived.
    public List<(TypedValue Row, object? Decoded)>? CapturedTree { get; private set; }

    public string? Error { get; set; }

    // Called from inside the game's binding update: an exception escaping into native code would take the game down.
    public void OnValue(Bindings.Key key, TypedValue value)
    {
        try
        {
            CallbackCount++;
            LastCallbackUtc = DateTime.UtcNow;
            ValueKey = key;
            Value = TypedValueReader.Read(value);

            if (capture == PropertyCapture.Value)
            {
                CapturedValue = TypedValue.CreateCopyFrom(value);
            }
            else if (capture == PropertyCapture.Items)
            {
                CapturedItems = CopyItems(value);
            }
            else if (capture == PropertyCapture.Tree)
            {
                CapturedTree = CopyTree(value);
            }

            Received = true;
        }
        catch (Exception exception)
        {
            Error = $"Reading the value failed: {exception.Message}";
        }
    }

    private static List<TypedValue>? CopyItems(TypedValue value)
    {
        var list = TypedValueReader.AsList(value);

        if (list is null)
        {
            return null;
        }

        var copies = new List<TypedValue>();

        for (var i = 0; i < list.Count; i++)
        {
            copies.Add(TypedValue.CreateCopyFrom(list[i]));
        }

        return copies;
    }

    private static List<(TypedValue Row, object? Decoded)>? CopyTree(TypedValue value)
    {
        var list = TypedValueReader.AsList(value);

        if (list is null)
        {
            return null;
        }

        var rows = new List<(TypedValue Row, object? Decoded)>();
        var level = new List<TypedValue>();

        for (var i = 0; i < list.Count; i++)
        {
            level.Add(list[i]);
        }

        for (var depth = 0; level.Count > 0 && depth < TypedValueReader.MaxTreeDepth; depth++)
        {
            var next = new List<TypedValue>();

            foreach (var row in level)
            {
                rows.Add((TypedValue.CreateCopyFrom(row), TypedValueReader.Read(row)));
                next.AddRange(TypedValueReader.ChildRows(row));
            }

            level = next;
        }

        return rows;
    }
}
