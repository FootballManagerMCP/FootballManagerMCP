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
    private const int MaxDepth = 3;
    private const int MaxListItems = 200;

    // Main thread only, like everything that reads game values.
    private static readonly Dictionary<IntPtr, int> NumberOffsets = [];

    public static object? Read(TypedValue? value, int depth = 0)
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

        return ReadContent(ContentOf(value), value, depth);
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

    public static TypedValueList? AsList(TypedValue? value)
    {
        return value is null ? null : ContentOf(value)?.TryCast<TypedValueList>();
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

    private static object? ReadContent(Il2CppObject? content, TypedValue owner, int depth)
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

        var reference = content.TryCast<InteropReference>();

        if (reference is not null)
        {
            return ReadReference(reference);
        }

        var list = content.TryCast<TypedValueList>();

        if (list is not null)
        {
            return ReadList(list, depth);
        }

        return new Dictionary<string, object?> { ["$type"] = type.FullName };
    }

    private static Dictionary<string, object?> ReadReference(InteropReference reference)
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

    private static object ReadList(TypedValueList list, int depth)
    {
        var count = list.Count;

        if (depth >= MaxDepth)
        {
            return new Dictionary<string, object?> { ["$count"] = count };
        }

        var items = new List<object?>();

        for (var i = 0; i < Math.Min(count, MaxListItems); i++)
        {
            items.Add(Read(list[i], depth + 1));
        }

        return count <= MaxListItems ? items : new Dictionary<string, object?> { ["$count"] = count, ["items"] = items };
    }

    private static object? Finite(double number)
    {
        return double.IsFinite(number) ? number : null;
    }
}
