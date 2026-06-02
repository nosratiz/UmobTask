namespace GbfsQuiz.Application.Features.Gbfs.Models;

/// <summary>
/// A bike-share system from the official GBFS systems list. <see cref="Id"/> is a
/// stable slug used as a cache/lookup key; <see cref="DiscoveryUrl"/> points at the
/// system's <c>gbfs.json</c> auto-discovery document.
/// </summary>
public sealed record GbfsProvider(string Id, string Name, string City, string DiscoveryUrl);
