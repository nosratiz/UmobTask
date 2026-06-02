using FluentResults;
using GbfsQuiz.Application.Common.Errors;
using GbfsQuiz.Application.Features.Auth.Interfaces;
using GbfsQuiz.Application.Features.Auth.Responses;
using GbfsQuiz.Domain.Players;

namespace GbfsQuiz.Application.Features.Auth;

/// <summary>Simple username/password account flow issuing bearer tokens.</summary>
public sealed class AuthService(
    IPlayerRepository players,
    IPasswordHasher passwordHasher,
    ITokenIssuer tokenIssuer) : IAuthService
{
    public async Task<Result<AuthResponse>> RegisterAsync(
        string username, string displayName, string password, CancellationToken ct = default)
    {
        var normalized = Normalize(username);
        if (await players.ExistsAsync(normalized, ct))
        {
            return Result.Fail<AuthResponse>(new ConflictError("That username is already taken."));
        }

        var player = new Player(normalized, displayName, passwordHasher.Hash(password));
        await players.CreateAsync(player, ct);

        return Result.Ok(BuildResponse(player));
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        string username, string password, CancellationToken ct = default)
    {
        var player = await players.FindByUsernameAsync(Normalize(username), ct);
        if (player is null || !passwordHasher.Verify(player.PasswordHash, password))
        {
            return Result.Fail<AuthResponse>(new UnauthorizedError("Invalid username or password."));
        }

        return Result.Ok(BuildResponse(player));
    }

    private AuthResponse BuildResponse(Player player)
    {
        var token = tokenIssuer.Issue(player);
        return new AuthResponse(
            player.Id, player.Username, player.DisplayName, token.Token, token.ExpiresAtUtc);
    }

    private static string Normalize(string username) => username.Trim().ToLowerInvariant();
}
