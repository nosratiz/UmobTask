namespace GbfsQuiz.Application.Features.Gbfs.Models;

/// <summary>
/// Point-in-time view of a single provider's network: the merged station list
/// plus the moment it was fetched. Aggregate helpers feed the quiz engine.
/// </summary>
public sealed record GbfsSnapshot(
    GbfsProvider Provider,
    IReadOnlyList<GbfsStation> Stations,
    DateTimeOffset FetchedAtUtc)
{
    public int StationCount => Stations.Count;

    /// <summary>Bikes actually available for rent — only counts installed, renting stations (GBFS spec).</summary>
    public int TotalBikesAvailable => Stations.Where(s => s.IsRentable).Sum(s => s.BikesAvailable);

    public int TotalDocksAvailable => Stations.Where(s => s.IsInstalled).Sum(s => s.DocksAvailable);

    public int TotalCapacity => Stations.Sum(s => s.Capacity);
}
