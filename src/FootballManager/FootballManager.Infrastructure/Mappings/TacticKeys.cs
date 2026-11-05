using System.Globalization;
using System.Text.Json;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Mappings;

// The game's English identifiers for what a tactic holds as numbers, from its enums (the plugin's /enums): style and
// mentality ids, formation ids, role masks. Keys are the enum names, trimmed of the prefixes that only repeat the
// type (Formation433DMWide is 433DMWide, OPTrackingCentreForward is TrackingCentreForward).
internal sealed class TacticKeys
{
    public required IReadOnlyDictionary<long, string> Styles { get; init; }
    public required IReadOnlyDictionary<long, string> Mentalities { get; init; }
    public required IReadOnlyDictionary<long, string> Formations { get; init; }
    public required IReadOnlyDictionary<ulong, string> InPossessionRoles { get; init; }
    public required IReadOnlyDictionary<ulong, string> OutOfPossessionRoles { get; init; }

    public IReadOnlyDictionary<ulong, string> Roles(bool inPossession)
    {
        return inPossession ? InPossessionRoles : OutOfPossessionRoles;
    }

    public static TacticKeys From(Dictionary<string, Dictionary<string, JsonElement>> enums)
    {
        Dictionary<string, JsonElement> Enum(string type)
        {
            return enums.TryGetValue(type, out var values) ? values : throw new InvalidOperationException($"The game sent no {type}.");
        }

        return new TacticKeys
        {
            Styles = Signed(Enum(TacticConstants.StylesEnum), name => name),
            Mentalities = Signed(Enum(TacticConstants.MentalitiesEnum), name => name),
            Formations = Signed(Enum(TacticConstants.FormationsEnum), name => Trim(name, TacticConstants.FormationPrefix)),
            InPossessionRoles = Roles(Enum(TacticConstants.InPossessionRolesEnum), name => name),
            OutOfPossessionRoles = Roles(Enum(TacticConstants.OutOfPossessionRolesEnum),
                name => Trim(name, TacticConstants.OutOfPossessionRolePrefix))
        };
    }

    /// <summary>The id a key names, or null.</summary>
    public static TId? IdOf<TId>(IReadOnlyDictionary<TId, string> keys, string key) where TId : struct
    {
        return keys.Where(pair => pair.Value == key).Select(pair => (TId?)pair.Key).FirstOrDefault();
    }

    /// <summary>"PI_SHOOT_MORE" as "ShootMore", like the other keys.</summary>
    public static string PlayerInstructionKey(string name)
    {
        var words = Trim(name, TacticConstants.PlayerInstructionPrefix).Split('_', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(words.Select(word => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word.ToLowerInvariant())));
    }

    // Aliases share a value; the first declared name is the one kept.
    private static Dictionary<long, string> Signed(Dictionary<string, JsonElement> values, Func<string, string> key)
    {
        var keys = new Dictionary<long, string>();

        foreach (var (name, value) in values)
        {
            if (value.TryGetInt64(out var id))
            {
                keys.TryAdd(id, key(name));
            }
        }

        return keys;
    }

    // Only the roles themselves: not "no role", nor the duty flags the same enums carry.
    private static Dictionary<ulong, string> Roles(Dictionary<string, JsonElement> values, Func<string, string> key)
    {
        var keys = new Dictionary<ulong, string>();

        foreach (var (name, value) in values)
        {
            var role = key(name);

            if (role != TacticConstants.NoRole && !role.EndsWith(TacticConstants.DutySuffix, StringComparison.Ordinal)
                && value.TryGetUInt64(out var mask) && mask != 0)
            {
                keys.TryAdd(mask, role);
            }
        }

        return keys;
    }

    private static string Trim(string name, string prefix)
    {
        return name.StartsWith(prefix, StringComparison.Ordinal) && name.Length > prefix.Length ? name[prefix.Length..] : name;
    }
}
