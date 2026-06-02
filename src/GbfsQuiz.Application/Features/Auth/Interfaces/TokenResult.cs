namespace GbfsQuiz.Application.Features.Auth.Interfaces;

/// <summary>An issued bearer token and its expiry.</summary>
public sealed record TokenResult(string Token, DateTimeOffset ExpiresAtUtc);
