using System.Text.Json;
using System.Text.Json.Serialization;

namespace FootballManager.Infrastructure.Serialization;

// The game sends a player's season numbers either as a plain number (goals, appearances) or as a small record holding
// the value and how many matches it covers ({"PropertyValue": 3.26, "Count": 13}); this reads both as the value.
internal sealed class StatValueJsonConverter : JsonConverter<double?>
{
    private const string ValueField = "PropertyValue";

    public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return reader.GetDouble();
            case JsonTokenType.StartObject:
                using (var record = JsonDocument.ParseValue(ref reader))
                {
                    return record.RootElement.TryGetProperty(ValueField, out var value) && value.ValueKind == JsonValueKind.Number
                        ? value.GetDouble()
                        : null;
                }
            default:
                reader.Skip();
                return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteNumberValue(value.Value);
        }
    }
}
