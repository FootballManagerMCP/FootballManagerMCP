using SI.Bindable;
using SI.Core;
using DateTime = System.DateTime;

namespace FootballManager.Plugin.Data;

// One requested path within a PropertyRequest. The value is only valid inside the callback (the game pools values),
// so everything needed later is copied there.
internal sealed class PropertyBinding(string path, bool copyItems)
{
    public string Path { get; } = path;

    public Bindings.Key Key { get; set; }

    // Kept referenced for the binding's lifetime: the game holds only the IL2CPP side of the delegate.
    public Bindings.ValueChangedCallback? Callback { get; set; }

    public bool Received { get; private set; }

    public int CallbackCount { get; private set; }

    public DateTime LastCallbackUtc { get; private set; }

    public object? Value { get; private set; }

    // For a list-valued path that feeds further requests (e.g. the squad), copies of its items.
    public List<TypedValue>? Items { get; private set; }

    public string? Error { get; set; }

    // Called from inside the game's binding update: an exception escaping into native code would take the game down.
    public void OnValue(Bindings.Key key, TypedValue value)
    {
        try
        {
            CallbackCount++;
            LastCallbackUtc = DateTime.UtcNow;
            Value = TypedValueReader.Read(value);

            if (copyItems)
            {
                Items = CopyItems(value);
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
}
