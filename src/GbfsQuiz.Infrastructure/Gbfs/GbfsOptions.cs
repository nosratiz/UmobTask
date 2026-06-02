using GbfsQuiz.Application.Features.Gbfs.Models;

namespace GbfsQuiz.Infrastructure.Gbfs;

/// <summary>Bound from the <c>Gbfs</c> configuration section.</summary>
public sealed class GbfsOptions
{
    public const string SectionName = "Gbfs";

    /// <summary>The GBFS systems to fetch, configured under <c>Gbfs:Providers</c> in appsettings.json.</summary>
    public List<GbfsProvider> Providers { get; init; } = [];

    /// <summary>How long a fetched snapshot stays cached. GBFS data refreshes ~every 60s.</summary>
    public int CacheSeconds { get; init; } = 45;

    /// <summary>Per-request timeout for a single GBFS HTTP call.</summary>
    public int TimeoutSeconds { get; init; } = 15;

    /// <summary>Number of automatic retries on transient HTTP failures (5xx, 408, network errors).</summary>
    public int RetryCount { get; init; } = 3;

    /// <summary>Base delay between retries; grows exponentially with jitter.</summary>
    public int RetryBaseDelayMilliseconds { get; init; } = 300;
}
