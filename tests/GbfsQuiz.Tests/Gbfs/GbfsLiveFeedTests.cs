using FluentAssertions;
using GbfsQuiz.Application.Features.Gbfs.Models;
using GbfsQuiz.Infrastructure.Gbfs;
using GbfsQuiz.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GbfsQuiz.Tests.Gbfs;

/// <summary>
/// Hits a real GBFS feed. Tagged Integration so it can be excluded in offline CI via
/// <c>dotnet test --filter Category!=Integration</c>.
/// </summary>
[Trait("Category", "Integration")]
public sealed class GbfsLiveFeedTests
{
    [Fact]
    public async Task GetSnapshotAsync_AgainstLiveCitiBike_ReturnsStationsWithBikes()
    {
        var provider = new GbfsProvider(
            "citibike-nyc", "Citi Bike", "New York City", "https://gbfs.citibikenyc.com/gbfs/gbfs.json");
        var client = new GbfsClient(
            new StubHttpClientFactory(new HttpClient()), NullLogger<GbfsClient>.Instance);

        var result = await client.GetSnapshotAsync(provider, CancellationToken.None);

        result.IsSuccess.Should().BeTrue(because: string.Join("; ", result.Errors.Select(e => e.Message)));
        result.Value.StationCount.Should().BeGreaterThan(100);
        result.Value.TotalBikesAvailable.Should().BeGreaterThan(0);
    }
}
