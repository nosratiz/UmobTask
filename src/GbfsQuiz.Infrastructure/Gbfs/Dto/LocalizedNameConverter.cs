using System.Text.Json;
using System.Text.Json.Serialization;

namespace GbfsQuiz.Infrastructure.Gbfs.Dto;

/// <summary>
/// GBFS v1/v2 expose <c>name</c> as a plain string; v3 changed it to an array of
/// localized objects (<c>[{ "text": "...", "language": "en" }]</c>). This converter
/// accepts either and yields the first available text.
/// </summary>
public sealed class LocalizedNameConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString() ?? string.Empty;
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            return FirstText(document.RootElement);
        }

        reader.Skip();
        return string.Empty;
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);

    private static string FirstText(JsonElement array)
    {
        foreach (var element in array.EnumerateArray())
        {
            if (element.TryGetProperty("text", out var text))
            {
                return text.GetString() ?? string.Empty;
            }
        }

        return string.Empty;
    }
}
