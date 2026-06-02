using GbfsQuiz.Domain.Games;

namespace GbfsQuiz.Application.Features.Games.Interfaces;

/// <summary>
/// Persistence boundary for <see cref="GameSession"/> aggregates. Each method is a
/// self-contained unit of work (it owns its DbContext), which keeps it safe to call
/// from concurrently-rendering Blazor components.
/// </summary>
public interface IGameSessionRepository
{
    Task CreateAsync(GameSession session, CancellationToken ct = default);

    /// <summary>Load scoped to the owning player; returns a detached entity.</summary>
    Task<GameSession?> GetForPlayerAsync(Guid gameId, Guid playerId, CancellationToken ct = default);

    /// <summary>Persist mutations made to a previously loaded session.</summary>
    Task UpdateAsync(GameSession session, CancellationToken ct = default);

    /// <summary>Read-only history, newest first.</summary>
    Task<IReadOnlyList<GameSession>> GetHistoryAsync(Guid playerId, CancellationToken ct = default);
}
