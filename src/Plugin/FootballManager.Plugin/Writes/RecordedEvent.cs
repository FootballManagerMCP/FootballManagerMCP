using FootballManager.Plugin.Data;
using SI.Core;
using SI.Interop;
using DateTime = System.DateTime;

namespace FootballManager.Plugin.Writes;

// Something the game's UI sent, copied as it went by: a tactics event, a native event or a binding write. Holds the
// values (still usable to send a similar one) and the game records it targets. Main thread only.
internal sealed class RecordedEvent(string type, string? key = null)
{
    public string Type { get; } = type;

    // What it's told apart by within its type, e.g. a native event's name or the property a binding write set.
    public string? Key { get; } = key;

    public DateTime Utc { get; } = DateTime.UtcNow;

    public Dictionary<string, TypedValue> Values { get; } = [];

    public Dictionary<string, InteropReference> References { get; } = [];

    // Decoded when recorded, since that's when the values are certain to be what the UI sent.
    public Dictionary<string, object?> Description { get; } = [];

    // Set when reading the event failed part-way; such an event is shown but never used as a template.
    public string? Error { get; private set; }

    public void AddValue(string name, TypedValue? value)
    {
        if (value is null)
        {
            Description[name] = null;
            return;
        }

        Values[name] = TypedValue.CreateCopyFrom(value);
        Description[name] = new Dictionary<string, object?>
        {
            ["type"] = value.GetIl2CppType().FullName,
            ["value"] = TypedValueReader.Read(value),
            ["shape"] = TypedValueShape.Describe(value)
        };
    }

    public void AddReference(string name, InteropReference? reference)
    {
        if (reference is null)
        {
            Description[name] = null;
            return;
        }

        References[name] = reference;
        Description[name] = TypedValueReader.ReadReference(reference);
    }

    // A plain field (a slot index, a position mask, a flag), kept as a game value like the others so it can serve as
    // a template too.
    public void AddPlain(string name, object value)
    {
        Values[name] = value switch
        {
            bool flag => TypedValue.Create(flag),
            int number => TypedValue.Create(number),
            uint number => TypedValue.Create(number),
            _ => throw new ArgumentException($"'{name}' is a {value.GetType().Name}, which isn't recorded as a value.")
        };

        Description[name] = new Dictionary<string, object?>
        {
            ["type"] = value.GetType().FullName,
            ["value"] = value
        };
    }

    // Something only worth showing (an enum's name, a binding key), not a value to send back.
    public void AddNote(string name, object? value)
    {
        Description[name] = value;
    }

    public void Fail(string message)
    {
        Error = message;
        Description["error"] = message;
    }
}
