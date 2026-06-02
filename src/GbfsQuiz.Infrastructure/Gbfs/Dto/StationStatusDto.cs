using System.Text.Json.Serialization;

namespace GbfsQuiz.Infrastructure.Gbfs.Dto;

/// <summary>
/// Wire shape of one station in the GBFS <c>station_status</c> feed. Handles the v3.x
/// rename of <c>num_bikes_available</c> → <c>num_vehicles_available</c>, and treats
/// <c>is_renting</c>/<c>is_installed</c> per spec (booleans in v3, 1/0 in v1/v2).
/// </summary>
public sealed class StationStatusDto
{
    [JsonPropertyName("station_id")] public string StationId { get; init; } = string.Empty;

    [JsonPropertyName("num_vehicles_available")] public int? NumVehiclesAvailable { get; init; }
    [JsonPropertyName("num_bikes_available")] public int? NumBikesAvailable { get; init; }
    [JsonPropertyName("num_docks_available")] public int NumDocksAvailable { get; init; }

    [JsonPropertyName("is_renting")]
    [JsonConverter(typeof(FlexibleBoolConverter))]
    public bool IsRenting { get; init; } = true;

    [JsonPropertyName("is_installed")]
    [JsonConverter(typeof(FlexibleBoolConverter))]
    public bool IsInstalled { get; init; } = true;

    /// <summary>Prefers the v3 field, falling back to the legacy one.</summary>
    public int BikesAvailable => NumVehiclesAvailable ?? NumBikesAvailable ?? 0;
}
