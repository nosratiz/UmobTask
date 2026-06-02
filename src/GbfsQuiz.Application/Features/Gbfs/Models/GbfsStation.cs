namespace GbfsQuiz.Application.Features.Gbfs.Models;

/// <summary>
/// A docking station merged from the GBFS <c>station_information</c> (static) and
/// <c>station_status</c> (real-time) feeds.
/// </summary>
public sealed record GbfsStation(
    string Id,
    string Name,
    double Latitude,
    double Longitude,
    int Capacity,
    int BikesAvailable,
    int DocksAvailable,
    bool IsRenting,
    bool IsInstalled)
{
    /// <summary>
    /// Per the GBFS spec, vehicles count as available for rental only when the station
    /// is installed and currently renting.
    /// </summary>
    public bool IsRentable => IsInstalled && IsRenting;
}

