using GbfsQuiz.Application.Features.Games.Interfaces;
using GbfsQuiz.Domain.Games;
using GbfsQuiz.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GbfsQuiz.Infrastructure.Games;

/// <summary>
/// EF Core-backed persistence for game sessions. Each method creates and disposes its
/// own context via the factory (see <see cref="EfRepository"/>) — safe under Blazor
/// Server's concurrent rendering.
/// </summary>
public sealed class GameSessionRepository(IDbContextFactory<AppDbContext> contextFactory)
    : EfRepository(contextFactory), IGameSessionRepository
{
    public Task CreateAsync(GameSession session, CancellationToken ct = default) =>
        ExecuteAsync(db => db.GameSessions.Add(session), ct);

    public Task<GameSession?> GetForPlayerAsync(
        Guid gameId, Guid playerId, CancellationToken ct = default) =>
        QueryAsync((db, c) => db.GameSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == gameId && x.PlayerId == playerId, c), ct);

    public Task UpdateAsync(GameSession session, CancellationToken ct = default) =>
        ExecuteAsync(db => db.GameSessions.Update(session), ct);

    public Task<IReadOnlyList<GameSession>> GetHistoryAsync(
        Guid playerId, CancellationToken ct = default) =>
        QueryAsync(async (db, c) => (IReadOnlyList<GameSession>)await db.GameSessions
            .AsNoTracking()
            .Where(x => x.PlayerId == playerId && x.Outcome != GameOutcome.InProgress)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(c), ct);
}
