using FluentResults;
using GbfsQuiz.Application.Common.Errors;
using GbfsQuiz.Application.Features.Auth.Interfaces;
using GbfsQuiz.Application.Features.Players.Interfaces;

namespace GbfsQuiz.Application.Features.Players;

/// <summary>
/// Validates avatar uploads (small images, common formats only), stores the file via
/// <see cref="IAvatarStorage"/>, and persists the resulting relative URL on the player.
/// </summary>
public sealed class AvatarService(IPlayerRepository players, IAvatarStorage storage) : IAvatarService
{
    private const int MaxBytes = 512 * 1024;
    private static readonly HashSet<string> AllowedTypes =
        new(StringComparer.OrdinalIgnoreCase) { "image/png", "image/jpeg", "image/webp", "image/gif" };

    public async Task<Result<string>> SetAvatarAsync(
        Guid playerId, byte[] data, string contentType, CancellationToken ct = default)
    {
        var validation = Validate(data, contentType);
        if (validation.IsFailed)
        {
            return validation;
        }

        var player = await players.GetByIdAsync(playerId, ct);
        if (player is null)
        {
            return Result.Fail(new NotFoundError("Player not found."));
        }

        var path = await storage.SaveAsync(data, contentType, player.AvatarPath, ct);
        player.SetAvatar(path);
        await players.UpdateAsync(player, ct);
        return Result.Ok(path);
    }

    public async Task<Result<string>> GetAvatarUrlAsync(Guid playerId, CancellationToken ct = default)
    {
        var player = await players.GetByIdAsync(playerId, ct);
        return player is { AvatarPath: { Length: > 0 } path }
            ? Result.Ok(path)
            : Result.Fail<string>(new NotFoundError("This player has no avatar."));
    }

    private static Result Validate(byte[] data, string contentType)
    {
        if (data is null || data.Length == 0)
        {
            return Result.Fail("Avatar file is empty.");
        }

        if (data.Length > MaxBytes)
        {
            return Result.Fail($"Avatar must be {MaxBytes / 1024} KB or smaller.");
        }

        return AllowedTypes.Contains(contentType)
            ? Result.Ok()
            : Result.Fail("Avatar must be a PNG, JPEG, WebP or GIF image.");
    }
}
