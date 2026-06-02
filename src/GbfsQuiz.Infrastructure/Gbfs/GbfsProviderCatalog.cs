using GbfsQuiz.Application.Features.Gbfs.Interfaces;
using GbfsQuiz.Application.Features.Gbfs.Models;
using Microsoft.Extensions.Options;

namespace GbfsQuiz.Infrastructure.Gbfs;

/// <summary>
/// Provides the configured GBFS systems, bound from the <c>Gbfs:Providers</c>
/// configuration section (see <c>appsettings.json</c>) which a future DB-backed
/// settings store could populate.
/// </summary>
public sealed class GbfsProviderCatalog(IOptions<GbfsOptions> options) : IGbfsProviderCatalog
{
    private readonly IReadOnlyList<GbfsProvider> _providers = options.Value.Providers;

    public IReadOnlyList<GbfsProvider> GetProviders() => _providers;
}
