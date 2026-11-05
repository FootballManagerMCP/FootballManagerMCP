using System.Runtime.InteropServices;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using SI.Core;
using SI.Interop;

namespace FootballManager.Plugin.Data;

// Reads a game object's instance fields straight from memory at their offsets, for objects no typed code knows (a UI
// event of a class nothing registered for). Never through property getters, which crashed the game when invoked by
// reflection, and never ToString on a struct: numbers, flags and enums are read as raw values, strings, game values
// (TypedValue) and records (InteropReference) are decoded, anything else is only named. Main thread only.
internal static class ObjectFieldReader
{
    private const int FieldAttributeStatic = 0x10;

    // Il2CppTypeEnum values.
    private const int TypeBoolean = 0x02;
    private const int TypeChar = 0x03;
    private const int TypeI1 = 0x04;
    private const int TypeU1 = 0x05;
    private const int TypeI2 = 0x06;
    private const int TypeU2 = 0x07;
    private const int TypeI4 = 0x08;
    private const int TypeU4 = 0x09;
    private const int TypeI8 = 0x0a;
    private const int TypeU8 = 0x0b;
    private const int TypeR4 = 0x0c;
    private const int TypeR8 = 0x0d;
    private const int TypeString = 0x0e;
    private const int TypeValueType = 0x11;
    private const int TypeGenericInstance = 0x15;

    // The object's fields from its own class up, stopping at the first base class whose name starts with stopAt (e.g.
    // "EventBase", whose pooling fields say nothing about the event).
    public static unsafe Dictionary<string, object?> Read(Il2CppObjectBase target, string stopAt)
    {
        var fields = new Dictionary<string, object?>();
        var pointer = target.Pointer;

        for (var klass = IL2CPP.il2cpp_object_get_class(pointer); klass != IntPtr.Zero; klass = IL2CPP.il2cpp_class_get_parent(klass))
        {
            var className = NameOf(klass);

            if (className.StartsWith(stopAt, StringComparison.Ordinal) || className == "Object")
            {
                break;
            }

            var iterator = IntPtr.Zero;
            IntPtr field;

            while ((field = IL2CPP.il2cpp_class_get_fields(klass, ref iterator)) != IntPtr.Zero)
            {
                if ((IL2CPP.il2cpp_field_get_flags(field) & FieldAttributeStatic) != 0)
                {
                    continue;
                }

                var name = Clean(Marshal.PtrToStringAnsi(IL2CPP.il2cpp_field_get_name(field)) ?? "?");

                try
                {
                    var address = (byte*)pointer + IL2CPP.il2cpp_field_get_offset(field);
                    fields.TryAdd(name, ReadField(address, IL2CPP.il2cpp_field_get_type(field)));
                }
                catch (Exception exception)
                {
                    fields.TryAdd(name, $"<unreadable: {exception.Message}>");
                }
            }
        }

        return fields;
    }

    private static unsafe object? ReadField(byte* address, IntPtr type)
    {
        var kind = IL2CPP.il2cpp_type_get_type(type);

        switch (kind)
        {
            case TypeBoolean:
                return *(bool*)address;
            case TypeChar:
                return *(char*)address;
            case TypeI1:
                return *(sbyte*)address;
            case TypeU1:
                return *address;
            case TypeI2:
                return *(short*)address;
            case TypeU2:
                return *(ushort*)address;
            case TypeI4:
                return *(int*)address;
            case TypeU4:
                return *(uint*)address;
            case TypeI8:
                return *(long*)address;
            case TypeU8:
                return *(ulong*)address;
            case TypeR4:
                return Finite(*(float*)address);
            case TypeR8:
                return Finite(*(double*)address);
        }

        var klass = IL2CPP.il2cpp_class_from_type(type);

        if (kind == TypeValueType || (kind == TypeGenericInstance && IL2CPP.il2cpp_class_is_valuetype(klass)))
        {
            if (IL2CPP.il2cpp_class_is_enum(klass))
            {
                return new Dictionary<string, object?>
                {
                    ["$enum"] = NameOf(klass),
                    ["value"] = ReadField(address, IL2CPP.il2cpp_class_enum_basetype(klass))
                };
            }

            // A binding key is its 64 bits; other structs are only named, since reading them means knowing their layout.
            return NameOf(klass) == "Key" ? *(ulong*)address : new Dictionary<string, object?> { ["$struct"] = NameOf(klass) };
        }

        var objectPointer = *(IntPtr*)address;

        if (objectPointer == IntPtr.Zero)
        {
            return null;
        }

        if (kind == TypeString)
        {
            return IL2CPP.Il2CppStringToManaged(objectPointer);
        }

        var value = new Il2CppSystem.Object(objectPointer);
        var typed = value.TryCast<TypedValue>();

        if (typed is not null)
        {
            return TypedValueReader.Read(typed);
        }

        var reference = value.TryCast<InteropReference>();

        return reference is not null
            ? TypedValueReader.ReadReference(reference)
            : new Dictionary<string, object?> { ["$type"] = NameOf(IL2CPP.il2cpp_object_get_class(objectPointer)) };
    }

    private static string NameOf(IntPtr klass)
    {
        return Marshal.PtrToStringAnsi(IL2CPP.il2cpp_class_get_name(klass)) ?? "?";
    }

    // "<Tactic>k__BackingField" -> "Tactic".
    private static string Clean(string name)
    {
        return name.StartsWith('<') && name.IndexOf('>') is > 1 and var end ? name[1..end] : name;
    }

    private static object? Finite(double number)
    {
        return double.IsFinite(number) ? number : null;
    }
}
