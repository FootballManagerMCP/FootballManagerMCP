using System.Text.Json;
using System.Text.Json.Serialization;

namespace FootballManager.Infrastructure.Serialization;

// The plugin stops reading nested values at a depth, and sends a list it stopped at as {"$count": n}. For lists that only
// go that deep at their ends (a menu's last entries have no entries below), that reads as an empty list.
internal sealed class CappedListJsonConverter<T> : JsonConverter<List<T>>
{
    public override List<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            return JsonSerializer.Deserialize<List<T>>(ref reader, options);
        }

        reader.Skip();
        return [];
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
