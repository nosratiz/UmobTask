using System.Security.Claims;
using GbfsQuiz.Web.Common.Http;
using GbfsQuiz.Web.Common.Security;
using GbfsQuiz.Application.Features.Players.Interfaces;

namespace GbfsQuiz.Web.Features.Players;

/// <summary>Endpoints for uploading and serving player avatars.</summary>
public static class PlayerEndpoints
{
    public static IEndpointRouteBuilder MapPlayerEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/players/me/avatar", UploadAvatarAsync)
            .RequireAuthorization()
            .RequireRateLimiting("game")
            .DisableAntiforgery()
            .WithTags("Players")
            .WithName("UploadAvatar");

        app.MapGet("/api/players/{playerId:guid}/avatar", GetAvatarAsync)
            .AllowAnonymous()
            .RequireRateLimiting("public")
            .WithTags("Players")
            .WithName("GetAvatar");

        return app;
    }

    private static async Task<IResult> UploadAvatarAsync(
        IFormFile file, ClaimsPrincipal user, IAvatarService avatars, CancellationToken ct)
    {
        if (!user.TryGetId(out var playerId))
        {
            return Results.Unauthorized();
        }

        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, ct);
        var result = await avatars.SetAvatarAsync(playerId, buffer.ToArray(), file.ContentType, ct);
        return result.ToHttpResult();
    }

    // The image itself is served as a static file under /avatars/; this endpoint stays for
    // a stable API surface and redirects to the current file (its name changes per upload).
    private static async Task<IResult> GetAvatarAsync(
        Guid playerId, IAvatarService avatars, CancellationToken ct)
    {
        var result = await avatars.GetAvatarUrlAsync(playerId, ct);
        return result.IsSuccess
            ? Results.Redirect(result.Value)
            : result.ToHttpResult();
    }
}
