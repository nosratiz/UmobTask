using FluentAssertions;
using GbfsQuiz.Application.Features.Gbfs.Models;
using GbfsQuiz.Infrastructure.Gbfs;
using GbfsQuiz.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GbfsQuiz.Tests.Gbfs;

public sealed class GbfsClientTests
{
    private static readonly GbfsProvider Provider =
        new("test", "Test Bikes", "Testville", "https://feeds.test/gbfs.json");

    [Fact]
    public async Task GetSnapshotAsync_MergesInformationAndStatus_ByStationId()
    {
        // Arrange
        var responses = new Dictionary<string, string>
        {
            ["https://feeds.test/gbfs.json"] = Discovery(),
            ["https://feeds.test/station_information.json"] = StationInformation(),
            ["https://feeds.test/station_status.json"] = StationStatus()
        };
        var client = new GbfsClient(
            new StubHttpClientFactory(new HttpClient(new StubHttpMessageHandler(responses))),
            NullLogger<GbfsClient>.Instance);

        // Act
        var result = await client.GetSnapshotAsync(Provider);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Stations.Should().HaveCount(2);
        result.Value.TotalBikesAvailable.Should().Be(7);
        result.Value.Stations.Single(s => s.Id == "a").BikesAvailable.Should().Be(5);
    }

    [Fact]
    public async Task GetSnapshotAsync_WhenDiscoveryUnreachable_FailsWithExternalServiceError()
    {
        var client = new GbfsClient(
            new StubHttpClientFactory(new HttpClient(new StubHttpMessageHandler(new Dictionary<string, string>()))),
            NullLogger<GbfsClient>.Instance);

        var result = await client.GetSnapshotAsync(Provider);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetSnapshotAsync_WithV3Feed_HandlesFlatDiscovery_LocalizedNames_AndRentableRules()
    {
        // v3: flat data.feeds, name as localized array, num_vehicles_available, boolean flags.
        var responses = new Dictionary<string, string>
        {
            ["https://feeds.test/gbfs.json"] =
                """
                { "version": "3.0", "data": { "feeds": [
                    { "name": "station_information", "url": "https://feeds.test/station_information.json" },
                    { "name": "station_status", "url": "https://feeds.test/station_status.json" }
                ] } }
                """,
            ["https://feeds.test/station_information.json"] =
                """
                { "data": { "stations": [
                    { "station_id": "a", "name": [{ "text": "Alpha", "language": "en" }], "lat": 40, "lon": -73, "capacity": 10 },
                    { "station_id": "b", "name": [{ "text": "Beta", "language": "en" }], "lat": 41, "lon": -74, "capacity": 20 }
                ] } }
                """,
            ["https://feeds.test/station_status.json"] =
                """
                { "data": { "stations": [
                    { "station_id": "a", "num_vehicles_available": 5, "num_docks_available": 5, "is_renting": true, "is_installed": true },
                    { "station_id": "b", "num_vehicles_available": 9, "num_docks_available": 1, "is_renting": true, "is_installed": false }
                ] } }
                """
        };
        var client = new GbfsClient(
            new StubHttpClientFactory(new HttpClient(new StubHttpMessageHandler(responses))),
            NullLogger<GbfsClient>.Instance);

        var result = await client.GetSnapshotAsync(Provider);

        result.IsSuccess.Should().BeTrue();
        result.Value.Stations.Single(s => s.Id == "a").Name.Should().Be("Alpha");
        // Station b is not installed, so its 9 vehicles are excluded from rentable availability.
        result.Value.TotalBikesAvailable.Should().Be(5);
    }

    private static string Discovery() =>
        """
        { "version": "2.3", "data": { "en": { "feeds": [
            { "name": "station_information", "url": "https://feeds.test/station_information.json" },
            { "name": "station_status", "url": "https://feeds.test/station_status.json" }
        ] } } }
        """;

    private static string StationInformation() =>
        """
        { "data": { "stations": [
            { "station_id": "a", "name": "Alpha", "lat": 40.0, "lon": -73.0, "capacity": 10 },
            { "station_id": "b", "name": "Beta", "lat": 41.0, "lon": -74.0, "capacity": 20 }
        ] } }
        """;

    private static string StationStatus() =>
        """
        { "data": { "stations": [
            { "station_id": "a", "num_bikes_available": 5, "num_docks_available": 5, "is_renting": 1 },
            { "station_id": "b", "num_bikes_available": 2, "num_docks_available": 18, "is_renting": true }
        ] } }
        """;
}
