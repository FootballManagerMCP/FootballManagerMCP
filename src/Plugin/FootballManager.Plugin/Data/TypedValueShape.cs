using FootballManager.Plugin.Inspection;
using SI.Core;
using TypedValueList = Il2CppSystem.Collections.Generic.List<SI.Core.TypedValue>;

namespace FootballManager.Plugin.Data;

// The game types a value is built from, e.g. Ref<DynamicReference>{DragData: Num(SignedWhole), DragDropExtraData1:
// Val<Boolean>}. A value the plugin builds itself has to match the game's own, so the two are compared through this.
// Like TypedValueReader, it never calls ToString on game objects: only class names and decoded numbers.
internal static class TypedValueShape
{
    private const int MaxDepth = 4;

    public static string Describe(TypedValue? value)
    {
        return Describe(value, 0);
    }

    private static string Describe(TypedValue? value, int depth)
    {
        if (value is null)
        {
            return "null";
        }

        var numeric = value.TryCast<NumericTypedValue>();

        if (numeric is not null)
        {
            return $"Num({TypedValueReader.NumberKindOf(numeric)})";
        }

        var reference = value.TryCast<ReferenceTypedValue>();
        var holder = reference is not null ? "Ref" : "Val";
        var content = reference is not null ? reference.m_value : value.Get();

        if (content is null)
        {
            return $"{holder}<null>";
        }

        var shape = $"{holder}<{content.GetIl2CppType().Name}>";

        if (depth >= MaxDepth)
        {
            return shape;
        }

        var list = content.TryCast<TypedValueList>();

        if (list is not null)
        {
            return list.Count == 0 ? $"{shape}[]" : $"{shape}[{list.Count} x {Describe(list[0], depth + 1)}]";
        }

        var record = content.TryCast<Record>();

        if (record is null)
        {
            return shape;
        }

        var fields = new List<string>();

        foreach (var pair in record)
        {
            fields.Add($"{FieldName(pair.Key)}: {Describe(pair.Value, depth + 1)}");
        }

        return $"{shape}{{{string.Join(", ", fields)}}}";
    }

    // A name several ids share (the property Index and the context Index) gets its id, so shapes compare exactly.
    private static string FieldName(uint id)
    {
        var name = IdentifierCatalog.NameOf(id);
        return IdentifierCatalog.IsAmbiguous(name) ? $"{name}#{id}" : name;
    }
}
