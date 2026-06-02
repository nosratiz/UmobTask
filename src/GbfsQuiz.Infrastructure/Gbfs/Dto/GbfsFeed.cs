using System.Text.Json.Serialization;

namespace GbfsQuiz.Infrastructure.Gbfs.Dto;

/// <summary>Generic GBFS feed envelope: every feed wraps its payload under <c>data</c>.</summary>
public sealed class GbfsFeed<TData>
{
    [JsonPropertyName("data")] public TData? Data { get; init; }
}
