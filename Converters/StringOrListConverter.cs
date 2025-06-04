using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BE.Converters
{
    public class StringOrListConverter : JsonConverter<List<string>?>
    {
        public override List<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return new List<string>(); // Convert null to empty list
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                string? stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue) || stringValue.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                {
                    return new List<string>(); // Convert "N/A" or empty string to empty list
                }
                return new List<string> { stringValue }; // Convert single string to a list with one element
            }

            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var list = new List<string>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        string? element = reader.GetString();
                        if (!string.IsNullOrWhiteSpace(element))
                        {
                            list.Add(element);
                        }
                    }
                    else if (reader.TokenType == JsonTokenType.Null)
                    {
                        // Optionally, ignore null elements within the array or add an empty string
                    }
                    else
                    {
                        // Handle unexpected token types within the array (e.g., numbers, booleans)
                        // For robustness, you might want to log this or throw a more specific exception.
                        reader.Skip();
                    }
                }
                return list;
            }

            throw new JsonException($"Unexpected JSON token type for List<string>: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, List<string>? value, JsonSerializerOptions options)
        {
            if (value == null || !value.Any())
            {
                writer.WriteStringValue("N/A"); // Or writer.WriteNullValue() if you prefer null in JSON
            }
            else if (value.Count == 1)
            {
                writer.WriteStringValue(value[0]);
            }
            else
            {
                writer.WriteStartArray();
                foreach (var item in value)
                {
                    writer.WriteStringValue(item);
                }
                writer.WriteEndArray();
            }
        }
    }
}