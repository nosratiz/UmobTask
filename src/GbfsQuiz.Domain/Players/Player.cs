using GbfsQuiz.Domain.Common;

namespace GbfsQuiz.Domain.Players;

/// <summary>A registered account. Owns its credential hash; never stores raw passwords.</summary>
public sealed class Player : Entity
{
    private Player() { } // EF Core materialization.

    public Player(string username, string displayName, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        Username = username.Trim().ToLowerInvariant();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? Username : displayName.Trim();
        PasswordHash = passwordHash;
    }

    public string Username { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>Relative URL of the stored avatar image (e.g. <c>/avatars/{id}.png</c>); null if none.</summary>
    public string? AvatarPath { get; private set; }
    public bool HasAvatar => !string.IsNullOrWhiteSpace(AvatarPath);

    public void SetAvatar(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        AvatarPath = path;
    }
}
