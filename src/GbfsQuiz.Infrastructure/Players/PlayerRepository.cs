using GbfsQuiz.Application.Features.Auth.Interfaces;
using GbfsQuiz.Domain.Players;
using GbfsQuiz.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GbfsQuiz.Infrastructure.Players;

/// <summary>
/// EF Core-backed persistence for player accounts. Each method owns its DbContext via
/// the factory (see <see cref="EfRepository"/>) — safe under Blazor Server's concurrent rendering.
/// </summary>
public sealed class PlayerRepository(IDbContextFactory<AppDbContext> contextFactory)
    : EfRepository(contextFactory), IPlayerRepository
{
    public Task<bool> ExistsAsync(string username, CancellationToken ct = default) =>
        QueryAsync((db, c) => db.Players.AsNoTracking().AnyAsync(x => x.Username == username, c), ct);

    public Task<Player?> FindByUsernameAsync(string username, CancellationToken ct = default) =>
        QueryAsync((db, c) => db.Players.AsNoTracking().FirstOrDefaultAsync(x => x.Username == username, c), ct);

    public Task<Player?> GetByIdAsync(Guid playerId, CancellationToken ct = default) =>
        QueryAsync((db, c) => db.Players.AsNoTracking().FirstOrDefaultAsync(x => x.Id == playerId, c), ct);

    public Task CreateAsync(Player player, CancellationToken ct = default) =>
        ExecuteAsync(db => db.Players.Add(player), ct);

    public Task UpdateAsync(Player player, CancellationToken ct = default) =>
        ExecuteAsync(db => db.Players.Update(player), ct);
}
