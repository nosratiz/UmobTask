using System.Text.Json.Serialization;

namespace GbfsQuiz.Infrastructure.Gbfs.Dto;

/// <summary>Wire shape of one station in the GBFS <c>station_information</c> feed.</summary>
public sealed class StationInformationDto
{
    [JsonPropertyName("station_id")] public string StationId { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    [JsonConverter(typeof(LocalizedNameConverter))]
    public string Name { get; init; } = string.Empty;
    [JsonPropertyName("lat")] public double Lat { get; init; }
    [JsonPropertyName("lon")] public double Lon { get; init; }
    [JsonPropertyName("capacity")] public int Capacity { get; init; }
}
