using System.Text.Json;

namespace GbfsQuiz.Infrastructure.Gbfs;

/// <summary>
/// Parses the GBFS auto-discovery document (<c>gbfs.json</c>) into a feed-name → URL
/// map. Tolerates both the language-keyed shape (<c>data.en.feeds</c>, v1/v2) and the
/// flattened shape (<c>data.feeds</c>, v3).
/// </summary>
public static class GbfsDiscoveryParser
{
    public static IReadOnlyDictionary<string, string> ParseFeeds(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("data", out var data))
        {
            return new Dictionary<string, string>();
        }

        var feedsArray = ResolveFeedsArray(data);
        return feedsArray is null ? new Dictionary<string, string>() : ToMap(feedsArray.Value);
    }

    private static JsonElement? ResolveFeedsArray(JsonElement data)
    {
        if (data.TryGetProperty("feeds", out var direct))
        {
            return direct;
        }

        foreach (var language in data.EnumerateObject())
        {
            if (language.Value.TryGetProperty("feeds", out var feeds))
            {
                return feeds;
            }
        }

        return null;
    }

    private static Dictionary<string, string> ToMap(JsonElement feeds)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var feed in feeds.EnumerateArray())
        {
            if (feed.TryGetProperty("name", out var name) && feed.TryGetProperty("url", out var url))
            {
                map[name.GetString() ?? string.Empty] = url.GetString() ?? string.Empty;
            }
        }

        return map;
    }
}
