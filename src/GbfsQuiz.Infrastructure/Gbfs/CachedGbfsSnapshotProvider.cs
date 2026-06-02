using FluentResults;
using GbfsQuiz.Application.Common.Errors;
using GbfsQuiz.Application.Features.Gbfs.Interfaces;
using GbfsQuiz.Application.Features.Gbfs.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GbfsQuiz.Infrastructure.Gbfs;

/// <summary>
/// Caches the merged snapshots of every configured provider so a quiz round sees
/// consistent data and the upstream feeds aren't hammered. Degrades gracefully:
/// providers that fail are skipped as long as at least one succeeds.
/// </summary>
public sealed class CachedGbfsSnapshotProvider(
    IGbfsClient client,
    IGbfsProviderCatalog catalog,
    IMemoryCache cache,
    IOptions<GbfsOptions> options,
    ILogger<CachedGbfsSnapshotProvider> logger) : IGbfsSnapshotProvider
{
    private const string CacheKey = "gbfs:snapshots";

    public async Task<Result<IReadOnlyList<GbfsSnapshot>>> GetAllAsync(CancellationToken ct = default)
    {
        if (cache.TryGetValue(CacheKey, out IReadOnlyList<GbfsSnapshot>? cached) && cached is not null)
        {
            return Result.Ok(cached);
        }

        var snapshots = await FetchAllAsync(ct);
        if (snapshots.Count == 0)
        {
            return Result.Fail<IReadOnlyList<GbfsSnapshot>>(
                new ExternalServiceError("No GBFS provider could be reached."));
        }

        cache.Set(CacheKey, snapshots, TimeSpan.FromSeconds(options.Value.CacheSeconds));
        return Result.Ok(snapshots);
    }

    private async Task<IReadOnlyList<GbfsSnapshot>> FetchAllAsync(CancellationToken ct)
    {
        var tasks = catalog.GetProviders().Select(p => client.GetSnapshotAsync(p, ct));
        var results = await Task.WhenAll(tasks);

        foreach (var failed in results.Where(r => r.IsFailed))
        {
            logger.LogWarning("Skipping failed provider snapshot: {Errors}",
                string.Join("; ", failed.Errors.Select(e => e.Message)));
        }

        return results.Where(r => r.IsSuccess).Select(r => r.Value).ToList();
    }
}
