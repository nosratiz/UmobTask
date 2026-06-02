using GbfsQuiz.Application.Features.Auth.Interfaces;
using GbfsQuiz.Domain.Players;
using Microsoft.AspNetCore.Identity;

namespace GbfsQuiz.Infrastructure.Auth;

/// <summary>Wraps ASP.NET Core Identity's PBKDF2 hasher behind the app abstraction.</summary>
public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<Player> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(DummyUser, password);

    public bool Verify(string passwordHash, string providedPassword) =>
        _hasher.VerifyHashedPassword(DummyUser, passwordHash, providedPassword)
            is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;

    // The default Identity hasher ignores the user argument entirely.
    private static readonly Player DummyUser = new("placeholder", "placeholder", "placeholder");
}
