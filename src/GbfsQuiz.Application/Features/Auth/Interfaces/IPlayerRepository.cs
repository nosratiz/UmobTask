using GbfsQuiz.Domain.Players;

namespace GbfsQuiz.Application.Features.Auth.Interfaces;

/// <summary>
/// Persistence boundary for player accounts. Each method is a self-contained unit of
/// work, safe to call from concurrently-rendering Blazor components.
/// </summary>
public interface IPlayerRepository
{
    Task<bool> ExistsAsync(string username, CancellationToken ct = default);

    Task<Player?> FindByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>Load by id; returns a detached entity.</summary>
    Task<Player?> GetByIdAsync(Guid playerId, CancellationToken ct = default);

    Task CreateAsync(Player player, CancellationToken ct = default);

    /// <summary>Persist mutations made to a previously loaded player (e.g. an avatar).</summary>
    Task UpdateAsync(Player player, CancellationToken ct = default);
}
