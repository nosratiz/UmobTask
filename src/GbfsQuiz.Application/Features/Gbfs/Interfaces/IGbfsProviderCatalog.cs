using GbfsQuiz.Application.Features.Gbfs.Models;

namespace GbfsQuiz.Application.Features.Gbfs.Interfaces;

/// <summary>Source of the configured GBFS providers the game draws questions from.</summary>
public interface IGbfsProviderCatalog
{
    IReadOnlyList<GbfsProvider> GetProviders();
}
