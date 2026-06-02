using GbfsQuiz.Web.Common.Http;
using GbfsQuiz.Application.Features.Leaderboard.Interfaces;

namespace GbfsQuiz.Web.Features.Leaderboard;

/// <summary>Public, anonymous endpoint exposing the multiplayer leaderboard.</summary>
public static class LeaderboardEndpoints
{
    public static IEndpointRouteBuilder MapLeaderboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/leaderboard",
                async (ILeaderboardService leaderboard, int? limit, CancellationToken ct) =>
                    (await leaderboard.GetTopAsync(limit ?? 10, ct)).ToHttpResult())
            .WithTags("Leaderboard")
            .AllowAnonymous()
            .RequireRateLimiting("public")
            .WithName("Leaderboard");

        return app;
    }
}
