using FluentResults;

namespace GbfsQuiz.Application.Features.Players.Interfaces;

/// <summary>Stores player avatar images and resolves their URLs.</summary>
public interface IAvatarService
{
    /// <summary>Validates and stores an uploaded image; returns the relative URL it is served from.</summary>
    Task<Result<string>> SetAvatarAsync(
        Guid playerId, byte[] data, string contentType, CancellationToken ct = default);

    /// <summary>Returns the relative URL of the player's avatar, or a NotFoundError if they have none.</summary>
    Task<Result<string>> GetAvatarUrlAsync(Guid playerId, CancellationToken ct = default);
}
