using System.Text.Json.Serialization;

namespace GbfsQuiz.Infrastructure.Gbfs.Dto;

/// <summary>The <c>data</c> payload of the station feeds: a list of stations.</summary>
public sealed class StationCollection<TStation>
{
    [JsonPropertyName("stations")] public List<TStation> Stations { get; init; } = [];
}
