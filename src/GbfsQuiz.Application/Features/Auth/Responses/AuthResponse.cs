namespace GbfsQuiz.Application.Features.Auth.Responses;

/// <summary>Issued on successful registration or login. The token authorises game calls.</summary>
public sealed record AuthResponse(
    Guid PlayerId,
    string Username,
    string DisplayName,
    string Token,
    DateTimeOffset ExpiresAtUtc);
