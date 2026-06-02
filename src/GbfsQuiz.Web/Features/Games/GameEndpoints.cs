using System.Security.Claims;
using System.Text;
using GbfsQuiz.Web.Common.Http;
using GbfsQuiz.Web.Common.Security;
using GbfsQuiz.Web.Common.Validation;
using GbfsQuiz.Application.Features.Games;
using GbfsQuiz.Application.Features.Games.Interfaces;
using GbfsQuiz.Application.Features.Games.Requests;

namespace GbfsQuiz.Web.Features.Games;

/// <summary>Minimal-API endpoints for the 60-second game loop and history.</summary>
public static class GameEndpoints
{
    public static IEndpointRouteBuilder MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/games")
            .WithTags("Games")
            .RequireAuthorization()
            .RequireRateLimiting("game");

        group.MapPost("/start", async (ClaimsPrincipal user, IGameService games, CancellationToken ct) =>
            !user.TryGetId(out var playerId)
                ? Results.Unauthorized()
                : (await games.StartAsync(playerId, ct)).ToHttpResult())
            .WithName("StartGame");

        group.MapPost("/{gameId:guid}/answers",
            async (Guid gameId, SubmitAnswerRequest body, ClaimsPrincipal user, IGameService games, CancellationToken ct) =>
                !user.TryGetId(out var playerId)
                    ? Results.Unauthorized()
                    : (await games.SubmitAnswerAsync(playerId, gameId, body.QuestionId, body.ChoiceId, ct)).ToHttpResult())
            .AddEndpointFilter<ValidationFilter<SubmitAnswerRequest>>()
            .WithName("SubmitAnswer");

        group.MapPost("/{gameId:guid}/finish",
            async (Guid gameId, ClaimsPrincipal user, IGameService games, CancellationToken ct) =>
                !user.TryGetId(out var playerId)
                    ? Results.Unauthorized()
                    : (await games.FinishAsync(playerId, gameId, ct)).ToHttpResult())
            .WithName("FinishGame");

        group.MapGet("/history", async (ClaimsPrincipal user, IGameService games, CancellationToken ct) =>
            !user.TryGetId(out var playerId)
                ? Results.Unauthorized()
                : (await games.GetHistoryAsync(playerId, ct)).ToHttpResult())
            .WithName("GameHistory");

        group.MapGet("/history/export", ExportHistoryAsync).WithName("ExportHistory");

        return app;
    }

    private static async Task<IResult> ExportHistoryAsync(
        ClaimsPrincipal user, IGameService games, CancellationToken ct)
    {
        if (!user.TryGetId(out var playerId))
        {
            return Results.Unauthorized();
        }

        var result = await games.GetHistoryAsync(playerId, ct);
        if (result.IsFailed)
        {
            return result.ToHttpResult();
        }

        var csv = HistoryCsvExporter.ToCsv(result.Value);
        return Results.File(Encoding.UTF8.GetBytes(csv), "text/csv", "gbfs-quiz-history.csv");
    }
}
