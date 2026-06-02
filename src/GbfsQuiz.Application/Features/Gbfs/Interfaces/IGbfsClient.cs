using FluentResults;
using GbfsQuiz.Application.Features.Gbfs.Models;

namespace GbfsQuiz.Application.Features.Gbfs.Interfaces;

/// <summary>Fetches and merges the live GBFS feeds for a single provider.</summary>
public interface IGbfsClient
{
    Task<Result<GbfsSnapshot>> GetSnapshotAsync(GbfsProvider provider, CancellationToken ct = default);
}
