using System.Globalization;
using System.Text.Json;
using FootballManager.Plugin.Inspection;
using SI.Bindable;
using SI.Core;

namespace FootballManager.Plugin.Writes;

// Builds a game value from JSON, for the record a native event carries as Data:
//   {"Name": ...}           a record keyed by property name, or by "#id" for one with no name; fields go in in the
//                           JSON's order. It's a DynamicReference, as the screens' rows are (a pick, a role), unless
//                           "$type": "Record" makes it a plain Record (the drag record, a role pick's outer record):
//                           the game serialises each by its own type, so match what /events shows as "shape".
//   true / false            a boolean
//   12                      a whole number, signed; "u:16384" makes it unsigned and "l:..." a long
//   "text"                  text
//   "@Path", "@List[F=v]"   the game's own value, read from the request's record like /set's values
// e.g. {"$type": "Record", "TeamSelectionIndex": 10, "Role": "@AvailableRolesDropdown[Role=2048].RowData", "IsInPossession": true}.
internal static class RecordBuilder
{
    private const char SourcePrefix = '@';
    private const string TypeField = "$type";
    private const string PlainRecord = "Record";
    private const string DynamicRecord = "DynamicReference";

    public static JsonElement Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.Clone();
        }
        catch (JsonException exception)
        {
            throw new ArgumentException($"The data isn't valid JSON: {exception.Message}");
        }
    }

    // The game values the JSON refers to, keyed by their text, to be read before building.
    public static Dictionary<string, ValueSource> Sources(JsonElement element)
    {
        var sources = new Dictionary<string, ValueSource>();
        CollectSources(element, sources);
        return sources;
    }

    private static void CollectSources(JsonElement element, Dictionary<string, ValueSource> sources)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    CollectSources(property.Value, sources);
                }

                break;
            case JsonValueKind.String when element.GetString() is [SourcePrefix, ..] text:
                sources[text] = ValueSource.Parse(text, text);
                break;
        }
    }

    // Main thread only: creates the game's values through its own factories.
    public static TypedValue Build(JsonElement element, IReadOnlyDictionary<string, TypedValue> resolved)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                return BuildRecord(element, resolved);
            case JsonValueKind.True:
            case JsonValueKind.False:
                return TypedValue.Create(element.GetBoolean());
            case JsonValueKind.Number:
                return element.TryGetInt32(out var number) ? TypedValue.Create(number)
                    : element.TryGetUInt32(out var unsigned) ? TypedValue.Create(unsigned)
                    : element.TryGetInt64(out var wide) ? TypedValue.Create(wide)
                    : throw new ArgumentException($"{element.GetRawText()} isn't a whole number the game takes.");
            case JsonValueKind.String:
                return Text(element.GetString()!, resolved);
            default:
                throw new ArgumentException($"A record holds records, booleans, whole numbers and text, not {element.ValueKind}.");
        }
    }

    private static TypedValue BuildRecord(JsonElement element, IReadOnlyDictionary<string, TypedValue> resolved)
    {
        var type = element.TryGetProperty(TypeField, out var typeElement) ? typeElement.GetString() : DynamicRecord;

        if (type is not (PlainRecord or DynamicRecord))
        {
            throw new ArgumentException($"A record's {TypeField} is {PlainRecord} or {DynamicRecord}, got '{type}'.");
        }

        var record = type == PlainRecord ? new Record() : new DynamicReference();

        foreach (var property in element.EnumerateObject().Where(p => p.Name != TypeField))
        {
            record.Add(IdOf(property.Name), Build(property.Value, resolved));
        }

        return record is DynamicReference dynamic ? TypedValue.Create<DynamicReference>(dynamic) : TypedValue.Create<Record>(record);
    }

    private static TypedValue Text(string text, IReadOnlyDictionary<string, TypedValue> resolved)
    {
        if (text.StartsWith(SourcePrefix))
        {
            return TypedValue.CreateCopyFrom(resolved[text]);
        }

        if (text.StartsWith("u:") && uint.TryParse(text[2..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var unsigned))
        {
            return TypedValue.Create(unsigned);
        }

        if (text.StartsWith("l:") && long.TryParse(text[2..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var wide))
        {
            return TypedValue.Create(wide);
        }

        return TypedValue.Create(text);
    }

    private static uint IdOf(string name)
    {
        if (name.StartsWith('#') && uint.TryParse(name[1..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
        {
            return id;
        }

        return IdentifierCatalog.TryIdOf(name) ?? throw new ArgumentException($"'{name}' isn't a game property (see /catalog?filter=).");
    }
}
