using FluentResults;
using GbfsQuiz.Application.Features.Gbfs.Models;

namespace GbfsQuiz.Application.Features.Gbfs.Interfaces;

/// <summary>
/// Supplies cached snapshots across all configured providers. Caching shields the
/// upstream feeds (and keeps a quiz round consistent) since GBFS data only refreshes
/// roughly once per minute.
/// </summary>
public interface IGbfsSnapshotProvider
{
    Task<Result<IReadOnlyList<GbfsSnapshot>>> GetAllAsync(CancellationToken ct = default);
}
