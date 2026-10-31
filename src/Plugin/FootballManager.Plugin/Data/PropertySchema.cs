using FM.UI;
using FootballManager.Plugin.Inspection;
using SI.Bindable.Reference.Core;
using ContextList = Il2CppSystem.Collections.Generic.List<SI.Bindable.Reference.Core.ContextID>;
using PropertyList = Il2CppSystem.Collections.Generic.List<SI.Bindable.Reference.Core.PropertyID>;

namespace FootballManager.Plugin.Data;

// What the game can tell about a record type: each property's value type, whether it's a list, which contexts it
// needs (e.g. a role) and, for the main types, its description.
internal static class PropertySchema
{
    private static readonly Dictionary<string, SchemaSource> Typed = new(StringComparer.OrdinalIgnoreCase)
    {
        ["person"] = new(
            PersonReference.GetPropertiesInternal,
            PersonReference.GetPropertyTypeInternal,
            PersonReference.GetContextsInternal,
            PersonReference.GetPropertyDescriptionInternal),
        ["team"] = new(
            TeamReference.GetPropertiesInternal,
            TeamReference.GetPropertyTypeInternal,
            TeamReference.GetContextsInternal,
            TeamReference.GetPropertyDescriptionInternal),
        ["club"] = new(
            ClubReference.GetPropertiesInternal,
            ClubReference.GetPropertyTypeInternal,
            ClubReference.GetContextsInternal,
            ClubReference.GetPropertyDescriptionInternal)
    };

    public static string Describe(string type, string? filter)
    {
        var source = Typed.TryGetValue(type, out var typed) ? typed : FromRegistry(type);
        var ids = new PropertyList();
        source.GetProperties(ids);

        var properties = new List<Dictionary<string, object?>>();

        for (var i = 0; i < ids.Count; i++)
        {
            var id = ids[i].id;
            var name = IdentifierCatalog.PropertyName(id);

            if (!string.IsNullOrWhiteSpace(filter) && !name.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            properties.Add(Describe(source, id, name));
        }

        return Json.Serialize(new Dictionary<string, object?>
        {
            ["type"] = type,
            ["count"] = properties.Count,
            ["properties"] = properties.OrderBy(p => (string)p["name"]!).ToList()
        });
    }

    private static Dictionary<string, object?> Describe(SchemaSource source, uint id, string name)
    {
        var kind = source.GetPropertyType(id);
        var contexts = new ContextList();
        source.GetContexts(id, contexts);

        var contextNames = new List<string>();

        for (var i = 0; i < contexts.Count; i++)
        {
            contextNames.Add(IdentifierCatalog.ContextName(contexts[i].id));
        }

        var referenceId = kind._Reference_k__BackingField.id;

        return new Dictionary<string, object?>
        {
            ["name"] = name,
            ["kind"] = kind._Kind_k__BackingField.ToString(),
            ["type"] = kind._Type_k__BackingField?.FullName,
            ["reference"] = referenceId == 0 ? null : IdentifierCatalog.ReferenceName(referenceId),
            ["isList"] = kind._IsList_k__BackingField,
            ["contexts"] = contextNames.Count == 0 ? null : contextNames,
            ["description"] = source.GetDescription?.Invoke(id)
        };
    }

    // Any other record type, through the registry (which has no descriptions).
    private static SchemaSource FromRegistry(string type)
    {
        foreach (var pair in ReferenceIdentifierSet.Instance.m_idToInfo)
        {
            var info = pair.Value;

            if (!string.Equals(info.Name, type, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return new SchemaSource(
                info.GetProperties,
                id => info.GetPropertyType(new PropertyID { id = id }),
                (id, contexts) => info.GetContexts(new PropertyID { id = id }, contexts),
                null);
        }

        throw new ArgumentException($"Unknown record type '{type}'. See /catalog?set=reference.");
    }
}
