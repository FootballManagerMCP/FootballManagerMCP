using System.Text.Json;

namespace FootballManager.Plugin.Data;

internal static class Json
{
    private static readonly JsonSerializerOptions Indented = new() { WriteIndented = true };

    public static string Serialize(object? value)
    {
        return JsonSerializer.Serialize(value, Indented);
    }

    public static string Compact(object? value)
    {
        return JsonSerializer.Serialize(value);
    }
}
