using System.Reflection;

namespace FootballManager.Plugin.Inspection;

// The names and values of the game's enums (e.g. FM.UI.InPossessionRole, whose values are the role masks the tactic
// holds), so callers can name things in English whatever language the game shows. Interop enums are plain managed
// enums: their constants are metadata, read without touching the game.
internal static class EnumCatalog
{
    public static Dictionary<string, Dictionary<string, object>> Describe(string? types)
    {
        var names = (types ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (names.Length == 0)
        {
            throw new ArgumentException("Name the enums by full name: types=FM.UI.InPossessionRole,FM.UI.TacticalStyles.");
        }

        return names.Distinct().ToDictionary(name => name, name => ValuesOf(Find(name)));
    }

    private static Type Find(string name)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? type;

            try
            {
                type = assembly.GetType(name, throwOnError: false);
            }
            catch (Exception)
            {
                continue;
            }

            if (type is not null)
            {
                return type.IsEnum ? type : throw new ArgumentException($"{name} isn't an enum.");
            }
        }

        throw new ArgumentException($"No loaded assembly has an enum named {name}.");
    }

    // In declared order, aliases included (several names can share a value).
    private static Dictionary<string, object> ValuesOf(Type type)
    {
        var unsigned = Enum.GetUnderlyingType(type) == typeof(ulong);

        return type.GetFields(BindingFlags.Public | BindingFlags.Static).ToDictionary(
            field => field.Name,
            field => unsigned
                ? (object)Convert.ToUInt64(field.GetRawConstantValue())
                : Convert.ToInt64(field.GetRawConstantValue()));
    }
}
