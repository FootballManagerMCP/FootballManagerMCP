using System.Text.Json;
using FootballManager.Domain.Players;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Mappings;

// Turns the attribute bound paths a player came back with into attributes; only the requested ones are present.
internal static class AttributeBoundsMapper
{
    public static List<PlayerAttribute> ToAttributes(Dictionary<string, JsonElement> bounds)
    {
        var attributes = new List<PlayerAttribute>();

        foreach (var definition in PlayerAttributes.All)
        {
            var hasMin = bounds.TryGetValue(GameConstants.AttributeMin(definition), out var min);
            var hasMax = bounds.TryGetValue(GameConstants.AttributeMax(definition), out var max);

            if (!hasMin && !hasMax)
            {
                continue;
            }

            attributes.Add(new PlayerAttribute
            {
                Name = definition.Name,
                Category = definition.Category,
                Min = Bound(min),
                Max = Bound(max)
            });
        }

        return attributes;
    }

    // Attributes run 1-20, so the 0 the game sends for a masked bound means "not available".
    private static int? Bound(JsonElement value)
    {
        return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) && number > 0 ? number : null;
    }
}
