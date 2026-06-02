using GbfsQuiz.Application.Features.Gbfs.Models;

namespace GbfsQuiz.Tests.Common;

/// <summary>Fluent factory for deterministic GBFS snapshots in tests.</summary>
public static class SnapshotBuilder
{
    public static GbfsSnapshot Snapshot(
        string id, string city, int stationCount, int bikesPerStation = 3)
    {
        var provider = new GbfsProvider(id, $"{city} Bikes", city, $"https://feeds/{id}.json");
        var stations = Enumerable.Range(0, stationCount)
            .Select(i => new GbfsStation(
                $"{id}-{i}", $"{city} St {i}", 40 + i * 0.01, -73 - i * 0.01,
                bikesPerStation * 2, bikesPerStation, bikesPerStation, IsRenting: true, IsInstalled: true))
            .ToList();
        return new GbfsSnapshot(provider, stations, DateTimeOffset.UtcNow);
    }
}
