using GbfsQuiz.Domain.Games;
using GbfsQuiz.Domain.Players;
using Microsoft.EntityFrameworkCore;

namespace GbfsQuiz.Infrastructure.Persistence;

/// <summary>
/// EF Core 10 unit of work. All mappings live in FluentConfigurations and are
/// discovered via assembly scanning — never via Data Annotations.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<Player> Players => Set<Player>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
