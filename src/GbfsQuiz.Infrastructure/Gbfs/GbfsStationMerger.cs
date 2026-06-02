using GbfsQuiz.Application.Features.Gbfs.Models;
using GbfsQuiz.Infrastructure.Gbfs.Dto;

namespace GbfsQuiz.Infrastructure.Gbfs;

/// <summary>Joins the static station information with real-time status by station id.</summary>
public static class GbfsStationMerger
{
    public static IReadOnlyList<GbfsStation> Merge(
        GbfsFeed<StationCollection<StationInformationDto>> information,
        GbfsFeed<StationCollection<StationStatusDto>> status)
    {
        var statusById = (status.Data?.Stations ?? [])
            .GroupBy(s => s.StationId)
            .ToDictionary(g => g.Key, g => g.First());

        return (information.Data?.Stations ?? [])
            .Select(info => ToStation(info, statusById))
            .ToList();
    }

    private static GbfsStation ToStation(
        StationInformationDto info, IReadOnlyDictionary<string, StationStatusDto> statusById)
    {
        statusById.TryGetValue(info.StationId, out var status);
        return new GbfsStation(
            info.StationId,
            info.Name,
            info.Lat,
            info.Lon,
            info.Capacity,
            status?.BikesAvailable ?? 0,
            status?.NumDocksAvailable ?? 0,
            status?.IsRenting ?? false,
            status?.IsInstalled ?? false);
    }
}
