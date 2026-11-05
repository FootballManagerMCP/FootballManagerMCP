using FM.GamePlugin;
using FM.UI;
using FootballManager.Plugin.Inspection;
using Il2CppInterop.Runtime;
using SI.Core;
using SI.Interop;
using Il2CppObject = Il2CppSystem.Object;
using TypedValueList = Il2CppSystem.Collections.Generic.List<SI.Core.TypedValue>;

namespace FootballManager.Plugin.Data;

// Turns a game TypedValue into plain .NET values for JSON, copied out at once: the game pools TypedValues and reuses
// them once a binding goes away.
//
// Never ToString a boxed struct here (interop passes the box as `this`, which reads garbage or crashes the game).
// Numbers come from DynamicNumber's fields, primitives are unboxed by type, references are read from their fields,
// and only enums go through the game's own AsString (called on the TypedValue, which is a class).
internal static class TypedValueReader
{
    // Deep enough for the formation picker's tree (categories, then formations, each a row record) and for rows
    // holding scored sub-records (a player's best positions in a tactic).
    private const int MaxDepth = 7;
    private const int MaxListItems = 200;

    // How many levels of nested rows a tree capture goes down (the preset picker has three: Config., style, preset).
    public const int MaxTreeDepth = 5;

    private const int CompactDepth = 2;

    // Main thread only, like everything that reads game values.
    private static readonly Dictionary<IntPtr, int> NumberOffsets = [];

    // A shallow read for values seen in bulk (the screens' data traffic): records and lists show only their top level.
    public static object? ReadCompact(TypedValue? value)
    {
        return Read(value, 0, CompactDepth);
    }

    public static object? Read(TypedValue? value, int depth = 0, int maxDepth = MaxDepth)
    {
        if (value is null)
        {
            return null;
        }

        var numeric = value.TryCast<NumericTypedValue>();

        if (numeric is not null)
        {
            return ReadNumber(NumberOf(numeric));
        }

        return ReadContent(ContentOf(value), value, depth, maxDepth);
    }

    // The interop accessor for TypedValue<DynamicNumber>.m_value reads the object header instead of the field, so the
    // field is located through the IL2CPP runtime (which resolves the generic layout) and read as raw memory.
    private static unsafe DynamicNumber NumberOf(NumericTypedValue numeric)
    {
        var klass = IL2CPP.il2cpp_object_get_class(numeric.Pointer);

        if (!NumberOffsets.TryGetValue(klass, out var offset))
        {
            var field = IL2CPP.il2cpp_class_get_field_from_name(klass, "m_value");

            if (field == IntPtr.Zero)
            {
                throw new InvalidOperationException("NumericTypedValue has no m_value field.");
            }

            offset = (int)IL2CPP.il2cpp_field_get_offset(field);
            NumberOffsets[klass] = offset;
        }

        return *(DynamicNumber*)((byte*)numeric.Pointer + offset);
    }

    public static DynamicNumber.NumberKind NumberKindOf(NumericTypedValue numeric)
    {
        return NumberOf(numeric)._Kind_k__BackingField;
    }

    public static TypedValueList? AsList(TypedValue? value)
    {
        return value is null ? null : ContentOf(value)?.TryCast<TypedValueList>();
    }

    // The fields of a record value (a dropdown row, a DynamicReference), or null for anything else. Numbers are
    // skipped: their content isn't read through Get() (see NumberOf).
    public static Record? AsRecord(TypedValue? value)
    {
        return value is null || value.TryCast<NumericTypedValue>() is not null ? null : ContentOf(value)?.TryCast<Record>();
    }

    // A field of a record value by property name (e.g. a dropdown row's RowData), or null if it has none.
    public static TypedValue? Field(TypedValue? value, string name)
    {
        var record = AsRecord(value);
        var id = IdentifierCatalog.TryIdOf(name);
        return record is not null && id is { } key && record.ContainsKey(key) ? record[key] : null;
    }

    // The database index of the record a value refers to (a person's, as dropdown rows give it as Index), or null.
    public static int? DatabaseIndexOf(TypedValue? value)
    {
        return AsReference(value)?.TryCast<DatabaseRecordReference>()?.m_index;
    }

    // The rows one level down from a tree row: the items of every list among its fields (a row keeps its children in a
    // field like ChildList), looking through nested records but not into the lists' items, which are the next level.
    public static List<TypedValue> ChildRows(TypedValue row)
    {
        var children = new List<TypedValue>();

        if (row.TryCast<NumericTypedValue>() is null)
        {
            CollectChildRows(ContentOf(row)?.TryCast<Record>(), children, 0);
        }

        return children;
    }

    private static void CollectChildRows(Record? fields, List<TypedValue> children, int depth)
    {
        if (fields is null || depth >= MaxDepth)
        {
            return;
        }

        foreach (var pair in fields)
        {
            var value = pair.Value;

            // Numbers hold no rows, and their content isn't read through Get() (see NumberOf).
            if (value is null || value.TryCast<NumericTypedValue>() is not null)
            {
                continue;
            }

            var content = ContentOf(value);
            var list = content?.TryCast<TypedValueList>();

            if (list is null)
            {
                CollectChildRows(content?.TryCast<Record>(), children, depth + 1);
                continue;
            }

            for (var i = 0; i < list.Count; i++)
            {
                children.Add(list[i]);
            }
        }
    }

    private static Il2CppObject? ContentOf(TypedValue value)
    {
        var reference = value.TryCast<ReferenceTypedValue>();
        return reference is not null ? reference.m_value : value.Get();
    }

    private static object? ReadNumber(DynamicNumber number)
    {
        return number._Kind_k__BackingField switch
        {
            DynamicNumber.NumberKind.SignedWhole => number.m_value.SignedWhole,
            DynamicNumber.NumberKind.UnsignedWhole => number.m_value.UnsignedWhole,
            DynamicNumber.NumberKind.FloatingPoint => Finite(number.m_value.FloatingPoint),
            _ => null
        };
    }

    private static object? ReadContent(Il2CppObject? content, TypedValue owner, int depth, int maxDepth)
    {
        if (content is null)
        {
            return null;
        }

        var type = content.GetIl2CppType();

        switch (type.FullName)
        {
            case "System.String":
                return RichText.StripRecordLinks(IL2CPP.Il2CppStringToManaged(content.Pointer) ?? "");
            case "System.Boolean":
                return content.Unbox<bool>();
            case "System.Byte":
                return content.Unbox<byte>();
            case "System.SByte":
                return content.Unbox<sbyte>();
            case "System.Int16":
                return content.Unbox<short>();
            case "System.UInt16":
                return content.Unbox<ushort>();
            case "System.Int32":
                return content.Unbox<int>();
            case "System.UInt32":
                return content.Unbox<uint>();
            case "System.Int64":
                return content.Unbox<long>();
            case "System.UInt64":
                return content.Unbox<ulong>();
            case "System.Single":
                return Finite(content.Unbox<float>());
            case "System.Double":
                return Finite(content.Unbox<double>());
        }

        if (type.IsEnum)
        {
            return owner.AsString();
        }

        if (type.FullName == "FM.GamePlugin.GameDate")
        {
            return ReadGameDate(content);
        }

        var reference = content.TryCast<InteropReference>();

        if (reference is not null)
        {
            return ReadReference(reference);
        }

        var list = content.TryCast<TypedValueList>();

        if (list is not null)
        {
            return ReadList(list, depth, maxDepth);
        }

        var fields = content.TryCast<Record>();

        if (fields is not null)
        {
            return ReadRecord(fields, depth, maxDepth);
        }

        return new Dictionary<string, object?> { ["$type"] = type.FullName };
    }

    // The game packs its dates; its own static ToDateTime unpacks one (a static call with a plain struct is safe, unlike
    // an instance call on a box), and the result's ticks are read straight from the struct.
    private static string ReadGameDate(Il2CppObject content)
    {
        var dateTime = GameDate.ToDateTime(content.Unbox<GameDate>());
        var ticks = (long)(dateTime._dateData & 0x3FFFFFFFFFFFFFFFUL);
        return new System.DateTime(ticks).ToString("yyyy-MM-dd");
    }

    /// <summary>The game record a value refers to (a tactic, a team), or null if it holds something else.</summary>
    public static InteropReference? AsReference(TypedValue? value)
    {
        return value is null ? null : ContentOf(value)?.TryCast<InteropReference>();
    }

    public static Dictionary<string, object?> ReadReference(InteropReference reference)
    {
        var typeName = reference.GetIl2CppType().Name;
        var result = new Dictionary<string, object?>
        {
            ["$ref"] = typeName.EndsWith("Reference") ? typeName[..^"Reference".Length] : typeName
        };

        var record = reference.TryCast<DatabaseRecordReference>();

        if (record is not null)
        {
            result["index"] = record.m_index;
        }

        var pairs = reference.m_id;

        if (pairs is not null && pairs.Count > 0)
        {
            var ids = new Dictionary<string, int>();

            for (var i = 0; i < pairs.Count; i++)
            {
                var pair = pairs[i];
                ids[IdentifierCatalog.NameOf(pair.m_id)] = pair.m_value;
            }

            result["ids"] = ids;
        }

        return result;
    }

    private static object ReadList(TypedValueList list, int depth, int maxDepth)
    {
        var count = list.Count;

        if (depth >= maxDepth)
        {
            return new Dictionary<string, object?> { ["$count"] = count };
        }

        var items = new List<object?>();

        for (var i = 0; i < Math.Min(count, MaxListItems); i++)
        {
            items.Add(Read(list[i], depth + 1, maxDepth));
        }

        return count <= MaxListItems ? items : new Dictionary<string, object?> { ["$count"] = count, ["items"] = items };
    }

    // The records the game builds for its screens (DynamicReference: a dropdown option, a table row) are plain maps of
    // property id to value.
    private static Dictionary<string, object?> ReadRecord(Record fields, int depth, int maxDepth)
    {
        if (depth >= maxDepth)
        {
            return new Dictionary<string, object?> { ["$fields"] = fields.Count };
        }

        var result = new Dictionary<string, object?>();

        foreach (var pair in fields)
        {
            result[IdentifierCatalog.NameOf(pair.Key)] = Read(pair.Value, depth + 1, maxDepth);
        }

        return result;
    }

    private static object? Finite(double number)
    {
        return double.IsFinite(number) ? number : null;
    }
}
