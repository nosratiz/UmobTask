using GbfsQuiz.Application.Features.Auth.Interfaces;
using GbfsQuiz.Application.Features.Gbfs.Interfaces;
using GbfsQuiz.Application.Features.Games.Interfaces;
using GbfsQuiz.Application.Features.Leaderboard.Interfaces;
using GbfsQuiz.Application.Features.Quiz.Interfaces;
using GbfsQuiz.Infrastructure.Auth;
using GbfsQuiz.Infrastructure.Gbfs;
using GbfsQuiz.Infrastructure.Games;
using GbfsQuiz.Infrastructure.Leaderboard;
using GbfsQuiz.Infrastructure.Persistence;
using GbfsQuiz.Infrastructure.Players;
using GbfsQuiz.Infrastructure.Quiz;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace GbfsQuiz.Infrastructure;

/// <summary>Composition root for the persistence and GBFS integration layers.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddGbfs(services, configuration);
        AddAuth(services, configuration);
        services.AddSingleton<IIssuedQuestionStore, MemoryIssuedQuestionStore>();
        services.AddScoped<IGameSessionRepository, GameSessionRepository>();
        services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();
        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = PostgresConnectionString.Normalize(
            configuration.GetConnectionString("Default")
            ?? "Host=localhost;Database=gbfs_quiz;Username=postgres;Password=postgres");

        // A factory (not a scoped DbContext) is the correct pattern for Blazor Server:
        // each repository operation gets its own short-lived context, so components that
        // render concurrently never share — and corrupt — a single DbContext instance.
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure()));

        // A scoped context resolved from the factory, used only by startup migration.
        services.AddScoped<AppDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());
    }

    private static void AddGbfs(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GbfsOptions>(configuration.GetSection(GbfsOptions.SectionName));
        services.AddMemoryCache();
        services.AddSingleton<IGbfsProviderCatalog, GbfsProviderCatalog>();
        services.AddScoped<IGbfsSnapshotProvider, CachedGbfsSnapshotProvider>();

        var options = configuration.GetSection(GbfsOptions.SectionName).Get<GbfsOptions>() ?? new GbfsOptions();

        var perAttempt = TimeSpan.FromSeconds(options.TimeoutSeconds);
        // The whole operation (all retries + back-off) gets one budget. Per-attempt × attempts
        // plus a small allowance for the back-off delays between them.
        var totalTimeout = perAttempt * (options.RetryCount + 1) + TimeSpan.FromSeconds(5);

        services.AddScoped<IGbfsClient, GbfsClient>();
        services.AddHttpClient(GbfsClient.ClientName, http =>
        {
            // Let the resilience pipeline own all timeouts. HttpClient.Timeout is a single
            // budget for the entire send (including retries), so leaving it at the default
            // 100s — or worse, the per-attempt value — would cancel the retries before they
            // can run. InfiniteTimeSpan hands timing control to the strategies below.
            http.Timeout = Timeout.InfiniteTimeSpan;
            http.DefaultRequestHeaders.Add("User-Agent", "GbfsQuiz/1.0");
        })
        // Polly v8 resilience: retry transient GBFS failures (5xx, 408, network errors,
        // per-attempt timeouts) with exponential back-off + jitter. A down feed is still
        // handled gracefully by CachedGbfsSnapshotProvider; this just smooths over blips.
        .AddResilienceHandler("gbfs", builder =>
        {
            // Outermost: a hard ceiling on the whole retried operation.
            builder.AddTimeout(totalTimeout);

            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = options.RetryCount,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromMilliseconds(options.RetryBaseDelayMilliseconds),
                ShouldHandle = static args => ValueTask.FromResult(
                    HttpClientResiliencePredicates.IsTransient(args.Outcome))
            });

            // Innermost: cap a single attempt so a hung connection can't stall the pipeline.
            builder.AddTimeout(perAttempt);
        });
    }

    private static void AddAuth(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddSingleton<ITokenIssuer, JwtTokenIssuer>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
    }
}
