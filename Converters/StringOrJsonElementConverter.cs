using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BE.Converters
{
    public class StringOrJsonElementConverter : JsonConverter<JsonElement>
    {
        public override JsonElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string? stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue) || stringValue.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                {
                    // Return a JsonElement that represents a JSON 'null'
                    using (JsonDocument document = JsonDocument.Parse("null"))
                    {
                        return document.RootElement.Clone();
                    }
                }
                // If it's a non-"N/A" string, return it as a JsonElement with string value
                using (JsonDocument document = JsonDocument.Parse($"\"{stringValue}\""))
                {
                    return document.RootElement.Clone();
                }
            }
            else if (reader.TokenType == JsonTokenType.Null)
            {
                // If JSON is explicitly null, return a JsonElement that represents a JSON 'null'
                using (JsonDocument document = JsonDocument.Parse("null"))
                {
                    return document.RootElement.Clone();
                }
            }
            else
            {
                // For other JSON token types (object, array, number, boolean),
                // read the whole element as a JsonElement.
                using (JsonDocument document = JsonDocument.ParseValue(ref reader))
                {
                    return document.RootElement.Clone();
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, JsonElement value, JsonSerializerOptions options)
        {
            // JsonElement's WriteTo method handles proper writing based on its ValueKind
            value.WriteTo(writer);
        }
    }
}