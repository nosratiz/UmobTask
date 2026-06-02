using System.Text.Json;
using System.Text.Json.Serialization;

namespace GbfsQuiz.Infrastructure.Gbfs.Dto;

/// <summary>
/// GBFS v1/v2 encode booleans like <c>is_renting</c> as <c>1</c>/<c>0</c>, while v3
/// uses real JSON booleans. This converter accepts either form.
/// </summary>
public sealed class FlexibleBoolConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number => reader.GetInt32() != 0,
            JsonTokenType.String => bool.TryParse(reader.GetString(), out var b) && b,
            _ => false
        };

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
        writer.WriteBooleanValue(value);
}
