namespace GbfsQuiz.Application.Features.Auth.Requests;

/// <summary>White-listed registration payload.</summary>
public sealed record RegisterRequest(string Username, string DisplayName, string Password);
