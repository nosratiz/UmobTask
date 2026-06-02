namespace GbfsQuiz.Application.Features.Auth.Requests;

/// <summary>White-listed login payload.</summary>
public sealed record LoginRequest(string Username, string Password);
