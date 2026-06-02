using Microsoft.EntityFrameworkCore;

namespace GbfsQuiz.Infrastructure.Persistence;

/// <summary>
/// Base for EF Core repositories. Each operation runs against a fresh, self-disposing
/// context from the factory — keeping the per-operation lifetime that makes the
/// repositories safe under Blazor Server's concurrent rendering, without hand-writing
/// the create/dispose dance in every method.
/// </summary>
public abstract class EfRepository(IDbContextFactory<AppDbContext> contextFactory)
{
    /// <summary>Runs a read against a fresh, self-disposing context.</summary>
    protected async Task<T> QueryAsync<T>(
        Func<AppDbContext, CancellationToken, Task<T>> work, CancellationToken ct)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        return await work(db, ct);
    }

    /// <summary>Applies a mutation and saves, against a fresh, self-disposing context.</summary>
    protected async Task ExecuteAsync(Action<AppDbContext> mutate, CancellationToken ct)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        mutate(db);
        await db.SaveChangesAsync(ct);
    }
}
