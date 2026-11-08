namespace FootballManager.Plugin.Writes;

// Blanks the credentials the game passes around in its events before a recording keeps them: one UI event carries an
// online service's client secret and the user's Steam id, and /events is read by tools and may end up anywhere.
internal static class Redaction
{
    private const string Redacted = "(redacted)";

    private static readonly string[] SensitiveNames = ["Secret", "Token", "Password", "UserId"];

    public static object? Apply(string name, object? value)
    {
        return IsSensitive(name) && value is not null ? Redacted : Apply(value);
    }

    // Decoded values: records are dictionaries, lists are lists (or a dictionary with their first items).
    public static object? Apply(object? value)
    {
        return value switch
        {
            Dictionary<string, object?> fields => fields.ToDictionary(pair => pair.Key, pair => Apply(pair.Key, pair.Value)),
            List<object?> items => items.Select(Apply).ToList(),
            _ => value
        };
    }

    private static bool IsSensitive(string name)
    {
        return SensitiveNames.Any(sensitive => name.Contains(sensitive, StringComparison.OrdinalIgnoreCase));
    }
}
