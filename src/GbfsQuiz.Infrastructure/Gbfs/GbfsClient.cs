using System.Text.Json;
using FluentResults;
using GbfsQuiz.Application.Common.Errors;
using GbfsQuiz.Application.Features.Gbfs.Interfaces;
using GbfsQuiz.Application.Features.Gbfs.Models;
using GbfsQuiz.Infrastructure.Gbfs.Dto;
using Microsoft.Extensions.Logging;

namespace GbfsQuiz.Infrastructure.Gbfs;

/// <summary>
/// Reads a provider's auto-discovery document, then fetches and merges its
/// <c>station_information</c> and <c>station_status</c> feeds into a single
/// <see cref="GbfsSnapshot"/>. The <see cref="HttpClient"/> is resolved from
/// <see cref="IHttpClientFactory"/> (named <see cref="ClientName"/>), so its
/// lifetime, pooling and resilience pipeline are factory-managed.
/// </summary>
public sealed class GbfsClient(IHttpClientFactory httpClientFactory, ILogger<GbfsClient> logger) : IGbfsClient
{
    /// <summary>Name of the configured <see cref="IHttpClientFactory"/> client (see DI registration).</summary>
    public const string ClientName = "gbfs";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient http = httpClientFactory.CreateClient(ClientName);

    public async Task<Result<GbfsSnapshot>> GetSnapshotAsync(GbfsProvider provider, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(provider);

        var feeds = await ResolveFeedsAsync(provider, ct);
        if (feeds.IsFailed)
        {
            return feeds.ToResult();
        }

        if (!TryGetFeedUrls(feeds.Value, out var infoUrl, out var statusUrl))
        {
            return Fail(provider, "discovery document is missing station feeds");
        }

        return await BuildSnapshotAsync(provider, infoUrl, statusUrl, ct);
    }

    private async Task<Result<IReadOnlyDictionary<string, string>>> ResolveFeedsAsync(
        GbfsProvider provider, CancellationToken ct)
    {
        var body = await GetStringAsync(provider.DiscoveryUrl, ct);
        return body.IsFailed
            ? body.ToResult()
            : Result.Ok(GbfsDiscoveryParser.ParseFeeds(body.Value));
    }

    private static bool TryGetFeedUrls(
        IReadOnlyDictionary<string, string> feeds, out string infoUrl, out string statusUrl) =>
        feeds.TryGetValue("station_information", out infoUrl!) &
        feeds.TryGetValue("station_status", out statusUrl!);

    private async Task<Result<GbfsSnapshot>> BuildSnapshotAsync(
        GbfsProvider provider, string infoUrl, string statusUrl, CancellationToken ct)
    {
        var infoTask = GetJsonAsync<GbfsFeed<StationCollection<StationInformationDto>>>(infoUrl, ct);
        var statusTask = GetJsonAsync<GbfsFeed<StationCollection<StationStatusDto>>>(statusUrl, ct);
        await Task.WhenAll(infoTask, statusTask);

        if (infoTask.Result.IsFailed || statusTask.Result.IsFailed)
        {
            return Fail(provider, "one or more station feeds could not be retrieved");
        }

        var stations = GbfsStationMerger.Merge(infoTask.Result.Value, statusTask.Result.Value);
        return Result.Ok(new GbfsSnapshot(provider, stations, DateTimeOffset.UtcNow));
    }

    private async Task<Result<T>> GetJsonAsync<T>(string url, CancellationToken ct)
    {
        var body = await GetStringAsync(url, ct);
        if (body.IsFailed)
        {
            return body.ToResult();
        }

        var value = JsonSerializer.Deserialize<T>(body.Value, Json);
        return value is null
            ? Result.Fail<T>(new ExternalServiceError($"GBFS feed at {url} returned an empty payload"))
            : Result.Ok(value);
    }

    private async Task<Result<string>> GetStringAsync(string url, CancellationToken ct)
    {
        try
        {
            var response = await http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            return Result.Ok(await response.Content.ReadAsStringAsync(ct));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Failed to fetch GBFS resource {Url}", url);
            return Result.Fail<string>(new ExternalServiceError($"Could not reach GBFS resource at {url}"));
        }
    }

    private Result<GbfsSnapshot> Fail(GbfsProvider provider, string reason)
    {
        logger.LogWarning("Snapshot for provider {ProviderId} failed: {Reason}", provider.Id, reason);
        return Result.Fail<GbfsSnapshot>(new ExternalServiceError($"GBFS snapshot for '{provider.Name}' failed: {reason}"));
    }
}
