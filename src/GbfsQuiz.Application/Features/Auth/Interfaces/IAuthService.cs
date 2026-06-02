using FluentResults;
using GbfsQuiz.Application.Features.Auth.Responses;

namespace GbfsQuiz.Application.Features.Auth.Interfaces;

/// <summary>Registration and login for the simple account system.</summary>
public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(
        string username, string displayName, string password, CancellationToken ct = default);

    Task<Result<AuthResponse>> LoginAsync(string username, string password, CancellationToken ct = default);
}
